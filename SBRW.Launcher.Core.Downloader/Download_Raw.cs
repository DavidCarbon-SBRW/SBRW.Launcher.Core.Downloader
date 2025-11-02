#if !TEST
using SBRW.Launcher.Core.Downloader.Extension_;
using SBRW.Launcher.Core.Downloader.Log_;
using SBRW.Launcher.Core.Downloader.Models_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// Configuration for the FileVerifier.
    /// </summary>
    public class Download_Raw_Config
    {
        /// <summary>
        /// The absolute path to the game directory.
        /// </summary>
        public string GamePath { get; set; } = string.Empty;
        /// <summary>
        /// The base URL for the CDN
        /// </summary>
        public string CdnBaseUrl { get; set; } = string.Empty;
        /// <summary>
        /// The name of the checksum manifest file (e.g., "checksums.dat").
        /// </summary>
        public string ChecksumFileName { get; set; } = "checksums.dat";
        /// <summary>
        /// A list of sub-paths within the CdnBaseUrl to try.
        /// </summary>
        public string CdnUnpackedPath { get; set; } = "/unpacked";
        /// <summary>
        /// Whether to delete all files in the 'scripts' folder
        /// </summary>
        public bool CleanScriptsFolder { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Download_Raw
    {
        private readonly Download_Raw_Config _config;
        private readonly Downloader_ILogger _logger;
        private readonly HttpClient _httpClient;
        /// <summary>
        /// 
        /// </summary>
        private struct FileChecksum
        {
            public string Hash;
            public string RelativePath;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="httpClient"></param>
        public Download_Raw(Download_Raw_Config config, Downloader_ILogger logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }
        /// <summary>
        /// Runs the entire verification, cleanup, and download process.
        /// </summary>
        public async Task<Download_Raw_Result_Model> RunAsync(IProgress<Download_Raw_Progress_Model> progress, CancellationToken token)
        {
            var result = new Download_Raw_Result_Model();
            try
            {
                await CleanupDirectoryAsync(progress, token);

                token.ThrowIfCancellationRequested();

                var checksums = await DownloadAndParseChecksumsAsync(progress, token);
                if (checksums == null)
                {
                    progress.Report(new Download_Raw_Progress_Model
                    {
                        Message = "Failed to download checksums. Stopping.",
                        Status = Download_Raw_Status.Download_Error_Checksums
                    });
                    return result; // Early exit
                }

                token.ThrowIfCancellationRequested();

                var invalidFiles = await ScanFilesAsync(checksums, result, progress, token);
                result.TotalFilesMissingOrInvalid = invalidFiles.Count;

                token.ThrowIfCancellationRequested();

                if (invalidFiles.Any())
                {
                    progress.Report(new Download_Raw_Progress_Model
                    {
                        Message = "Downloading invalid or missing files...",
                        Status = Download_Raw_Status.Download_Invalid_Files
                    });
                    await DownloadInvalidFilesAsync(invalidFiles, result, progress, token);
                }

                progress.Report(new Download_Raw_Progress_Model
                {
                    Message = "Verification complete.",
                    Status = Download_Raw_Status.Verification_Completed,
                    Percentage = 100
                });
            }
            catch (OperationCanceledException)
            {
                result.WasCancelled = true;
                progress.Report(new Download_Raw_Progress_Model 
                {
                    Message = "Operation cancelled by user.",
                    Status = Download_Raw_Status.Cancelled
                });
                _logger.Info("Verification process was cancelled by the user.");
            }
            catch (Exception ex)
            {
                progress.Report(new Download_Raw_Progress_Model
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = Download_Raw_Status.Error,
                });
                _logger.Error("An unhandled exception occurred during verification.", ex);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task CleanupDirectoryAsync(IProgress<Download_Raw_Progress_Model> progress, CancellationToken token)
        {
            progress.Report(new Download_Raw_Progress_Model 
            {
                Message = "Cleaning game directory...",
                Status = Download_Raw_Status.Symbolic_Orig_Cleanup
            });
            _logger.Info("Starting game directory cleanup.");

            var gameDir = new DirectoryInfo(_config.GamePath);
            if (!gameDir.Exists) return;

            // 1. Delete .orig files
            var origFiles = gameDir.EnumerateFiles("*.orig", SearchOption.AllDirectories);
            foreach (var file in origFiles)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    file.Delete();
                    _logger.Info($"Deleted '.orig' file: {file.FullName}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to delete file: {file.FullName}", ex);
                }
            }

            // 2. Delete symbolic links (More robust check)
            var allEntries = gameDir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories);
            foreach (var entry in allEntries)
            {
                token.ThrowIfCancellationRequested();
                if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    try
                    {
                        if (Directory.Exists(entry.FullName))
                        {
                            Directory.Delete(entry.FullName, true);
                        }
                        else
                        {
                            File.Delete(entry.FullName);
                        }
                        _logger.Info($"Deleted symbolic link: {entry.FullName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to delete symbolic link: {entry.FullName}", ex);
                    }
                }
            }

            // 3. Clean 'scripts' folder if configured
            if (_config.CleanScriptsFolder)
            {
                var scriptsDir = new DirectoryInfo(Path.Combine(_config.GamePath, "scripts"));
                if (scriptsDir.Exists)
                {
                    foreach (var file in scriptsDir.EnumerateFiles())
                    {
                        try
                        {
                            file.Delete();
                            _logger.Info($"Deleted script file: {file.Name}");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Failed to delete script file: {file.Name}", ex);
                        }
                    }
                }
            }

            _logger.Info("Cleanup complete.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<FileChecksum>?> DownloadAndParseChecksumsAsync(IProgress<Download_Raw_Progress_Model> progress, CancellationToken token)
        {
            progress.Report(new Download_Raw_Progress_Model
            {
                Message = "Downloading checksum manifest...",
                Status = Download_Raw_Status.Download_Checksums
            });

            string checksumUrl = $"{_config.CdnBaseUrl.TrimEnd('/')}/{_config.CdnUnpackedPath.Trim('/')}/{_config.ChecksumFileName}";
            string[] lines;

            try
            {
                using (var response = await _httpClient.GetAsync(checksumUrl, token))
                {
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to download checksum file from {checksumUrl}", ex);
                return null;
            }

            var checksums = new List<FileChecksum>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(new[] { ' ' }, 2);
                if (parts.Length == 2)
                {
                    checksums.Add(new FileChecksum
                    {
                        Hash = parts[0].Trim(),
                        RelativePath = parts[1].Trim()
                    });
                }
            }

            _logger.Info($"Parsed {checksums.Count} file checksums.");
            return checksums;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="checksums"></param>
        /// <param name="result"></param>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<string>> ScanFilesAsync(List<FileChecksum> checksums, Download_Raw_Result_Model result, IProgress<Download_Raw_Progress_Model> progress, CancellationToken token)
        {
            _logger.Info("Starting file scan...");
            var invalidFiles = new List<string>();
            int filesScanned = 0;

            foreach (var item in checksums)
            {
                token.ThrowIfCancellationRequested();
                filesScanned++;
                string localPath = Path.Combine(_config.GamePath, item.RelativePath.TrimStart('\\', '/'));

                if (!File.Exists(localPath))
                {
                    _logger.Warn($"Missing file: {item.RelativePath}");
                    invalidFiles.Add(item.RelativePath);
                }
                else
                {
                    string localHash = await CalculateFileHashAsync(localPath);
                    if (!localHash.Equals(item.Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Warn($"Invalid hash for file: {item.RelativePath} (Expected: {item.Hash}, Got: {localHash})");
                        invalidFiles.Add(item.RelativePath);
                    }
                }

                int percent = filesScanned * 100 / checksums.Count;
                progress.Report(new Download_Raw_Progress_Model
                {
                    Message = $"Scanning files: {percent}%",
                    Status = Download_Raw_Status.Scanning_Files,
                    CurrentFile = item.RelativePath,
                    CurrentFileNumber = filesScanned,
                    TotalFileNumber = (long)checksums.Count,
                    Percentage = percent
                });
            }

            result.TotalFilesScanned = filesScanned;
            _logger.Info($"Scan complete. Found {invalidFiles.Count} invalid or missing files.");
            return invalidFiles;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invalidFiles"></param>
        /// <param name="result"></param>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task DownloadInvalidFilesAsync(List<string> invalidFiles, Download_Raw_Result_Model result, IProgress<Download_Raw_Progress_Model> progress, CancellationToken token)
        {
            int filesDownloaded = 0;
            int totalToDownload = invalidFiles.Count;

            foreach (var relativePath in invalidFiles)
            {
                token.ThrowIfCancellationRequested();

                string fileUrl = $"{_config.CdnBaseUrl.TrimEnd('/')}/{_config.CdnUnpackedPath.Trim('/')}{relativePath.Replace("\\", "/")}";
                string localPath = Path.Combine(_config.GamePath, relativePath.TrimStart('\\', '/'));

                try
                {
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

                    // Delete old file if it exists
                    if (File.Exists(localPath))
                    {
                        File.Delete(localPath);
                    }

                    _logger.Info($"Downloading: {relativePath}");

                    // Download file with progress
                    await DownloadFileWithProgressAsync(fileUrl, localPath, relativePath, (filesDownloaded + 1), totalToDownload, progress, token);

                    filesDownloaded++;
                    result.FilesSuccessfullyDownloaded = filesDownloaded;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException) throw; // Re-throw cancellation

                    _logger.Error($"Failed to download file: {relativePath} from {fileUrl}", ex);
                    result.FilesFailedToDownload++;
                    result.FailedDownloadFiles.Add(relativePath);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="destinationPath"></param>
        /// <param name="relativePath"></param>
        /// <param name="currentFileNum"></param>
        /// <param name="totalFiles"></param>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task DownloadFileWithProgressAsync(string url, string destinationPath, string relativePath, int currentFileNum, int totalFiles, IProgress<Download_Raw_Progress_Model> progress, CancellationToken token)
        {
            using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                long totalBytesRead = 0;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                        totalBytesRead += bytesRead;

                        if (totalBytes.HasValue)
                        {
                            progress.Report(new Download_Raw_Progress_Model
                            {
                                Message = $"Downloading [{currentFileNum}/{totalFiles}]:",
                                Status = Download_Raw_Status.Downloading,
                                CurrentFile = relativePath,
                                CurrentFileNumber = currentFileNum,
                                Percentage = (int)(totalBytesRead * 100 / totalBytes.Value),
                                BytesDownloaded = totalBytesRead,
                                TotalBytesToDownload = totalBytes.Value,
                                TotalFileNumber = totalFiles
                            });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            string Converted_Hash = string.Empty;

            await Task.Run(() =>
            {
                Converted_Hash = Hashes.Hash_SHA(filePath).Trim();
            });

            return Converted_Hash;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public enum Download_Raw_Status
    {
        /// <summary>
        /// Failed to download checksums
        /// </summary>
        Download_Error_Checksums,
        /// <summary>
        /// Downloading invalid or missing files
        /// </summary>
        Download_Invalid_Files,
        /// <summary>
        /// Completed Verifying Files
        /// </summary>
        Verification_Completed,
        /// <summary>
        /// Operation cancelled by user
        /// </summary>
        Cancelled,
        /// <summary>
        /// An error occurred
        /// </summary>
        Error,
        /// <summary>
        /// Symbolic amd Orig Files Cleanup
        /// </summary>
        Symbolic_Orig_Cleanup,
        /// <summary>
        /// Downloading checksum manifest
        /// </summary>
        Download_Checksums,
        /// <summary>
        /// Scanning files
        /// </summary>
        Scanning_Files,
        /// <summary>
        /// Downloading Current Number over Total Number of Files
        /// </summary>
        Downloading,
        /// <summary>
        /// Default Download State
        /// </summary>
        Idle
    }
}
#endif