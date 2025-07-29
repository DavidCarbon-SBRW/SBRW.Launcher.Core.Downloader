#nullable enable
using SBRW.Launcher.Core.Downloader.EventArg_;
using SBRW.Launcher.Core.Downloader.Exception_;
using SBRW.Launcher.Core.Downloader.Extension_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// Provides functionality for downloading files with retry logic and progress tracking.
    /// </summary>
    public class Download_Client
    {
        private const int DefaultDownload_Block_Size = 8192; // Changed from 1KB to 8KB for better performance
        private const int DefaultDownload_Retry_Attempts = 10;
        private const long DefaultWeb_File_Size = 3862102244;
        private HttpWebResponse? _liveResponse;
        /// <summary>
        /// Read the file in chunks (in bytes). Default is 8KB.
        /// </summary>
        public int Download_Block_Size { get; set; } = DefaultDownload_Block_Size;
        /// <summary>
        /// Current download status information.
        /// </summary>
        public Download_Information? Download_Status_Information { get; internal set; }
        /// <summary>
        /// Retrieves the current download status information.
        /// </summary>
        public Download_Information? Download_Status() { return Download_Status_Information; }
        /// <summary>
        /// Number of download attempts. Default is 10.
        /// </summary>
        public int Download_Retry_Attempts { get; set; } = DefaultDownload_Retry_Attempts;
        /// <summary>
        /// Disables the updating of download status information.
        /// </summary>
        public bool Disable_Download_Status_Information { get; set; }
        /// <summary>
        /// Flag to cancel the current download operation.
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// Expected SHA1 hash of the file after download for validation.
        /// </summary>
        public string File_Hash { get; set; } = "88C886B6D131C052365C3D6D14E14F67A4E2C253";
        /// <summary>
        /// Local file name to be set. Default is "GameFiles.sbrwpack".
        /// </summary>
        public string File_Name { get; set; } = "GameFiles.sbrwpack";
        /// <summary>
        /// Current file size on local disk.
        /// </summary>
        public long File_Size { get; private set; } = 0;
        /// <summary>
        /// Live updated file size during download.
        /// </summary>
        public long File_Size_Live { get; internal set; } = 0;
        /// <summary>
        /// Base folder path for game files.
        /// </summary>
        public string Folder_Path { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game Files");
        /// <summary>
        /// Full path to the downloaded file.
        /// </summary>
        public string File_Path { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game Files", ".Launcher", "Downloads", "GameFiles.sbrwpack");
        /// <summary>
        /// Determines if an invalid file should be removed if its hash does not match.
        /// </summary>
        public bool File_Removal { get; set; } = false;
        /// <summary>
        /// Start time of the download operation.
        /// </summary>
        public DateTime Start_Time { get; private set; }
        /// <summary>
        /// Default web URL for downloads.
        /// </summary>
        public string Web_URL { get; set; } = "http://localhost";
        /// <summary>
        /// Expected file size on the web server.
        /// </summary>
        public long Web_File_Size { get; set; } = DefaultWeb_File_Size;
        /// <summary>
        /// Remaining file size to download from the web server.
        /// </summary>
        public long Web_File_Size_Remaining { get; internal set; } = 0;
        /// <summary>
        /// Event Delegate for reporting download completion.
        /// </summary>
        public delegate void Download_Data_Progress_Handler(object Sender, Download_Data_Progress_EventArgs Events);
        /// <summary>
        /// Event for reporting download progress.
        /// </summary>
        public event Download_Data_Progress_Handler? Live_Progress;
        /// <summary>
        /// Event Delegate for reporting download completion.
        /// </summary>
        public delegate void Download_Data_Completion_Handler(object Sender, Download_Data_Complete_EventArgs Events);
        /// <summary>
        /// Event for reporting download completion.
        /// </summary>
        public event Download_Data_Completion_Handler? Complete;
        /// <summary>
        /// Event Delegate for reporting internal errors during download.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Data_Exception_Handler(object Sender, Download_Exception_EventArgs Events);
        /// <summary>
        /// Event for reporting internal errors during download.
        /// </summary>
        public event Download_Data_Exception_Handler? Internal_Error;
        /// <summary>
        /// Routes exceptions to the Internal_Error event or re-throws them.
        /// </summary>
        /// <param name="hookEvent">If true, the exception is routed to the event. Otherwise, it's re-thrown.</param>
        /// <param name="exceptionCaught">The exception that was caught.</param>
        internal void Exception_Router(bool hookEvent, Exception exceptionCaught)
        {
            Exception_Router(hookEvent, exceptionCaught, false);
        }

        /// <summary>
        /// Routes exceptions to the Internal_Error event or re-throws them, indicating if it's web-related.
        /// </summary>
        /// <param name="hookEvent">If true, the exception is routed to the event. Otherwise, it's re-thrown.</param>
        /// <param name="exceptionCaught">The exception that was caught.</param>
        /// <param name="relatedToWebClient">True if the exception is related to the web client, otherwise false.</param>
        internal void Exception_Router(bool hookEvent, Exception exceptionCaught, bool relatedToWebClient)
        {
            _liveResponse?.Close();
            _liveResponse?.Dispose();
            _liveResponse = null;

            if (relatedToWebClient)
            {
                // Stop the download to prevent memory leaks if a web-related error occurs
                Cancel = true;
            }

            if (Internal_Error != null && hookEvent)
            {
                Internal_Error(this, new Download_Exception_EventArgs(exceptionCaught, DateTime.Now, relatedToWebClient));
            }
            else
            {
                throw exceptionCaught;
            }
        }

        /// <summary>
        /// Initiates a download with default parameters.
        /// </summary>
        public void Download()
        {
            Download(Web_URL, Folder_Path, File_Path, Web_File_Size, File_Name);
        }

        /// <summary>
        /// Initiates a download to a specified web address.
        /// </summary>
        /// <param name="webAddress">The URL of the file to download.</param>
        public void Download(string webAddress)
        {
            Download(webAddress, string.Empty);
        }

        /// <summary>
        /// Initiates a download to a specified web address and local folder.
        /// </summary>
        /// <param name="webAddress">The URL of the file to download.</param>
        /// <param name="locationFolder">The local folder to save the file.</param>
        public void Download(string webAddress, string locationFolder)
        {
            Download(webAddress, locationFolder, string.Empty);
        }

        /// <summary>
        /// Initiates a download with a specified web address, local folder, and archive file.
        /// </summary>
        /// <param name="webAddress">The URL of the file to download.</param>
        /// <param name="locationFolder">The local folder to save the file.</param>
        /// <param name="providedArchiveFile">The provided archive file path.</param>
        public void Download(string webAddress, string locationFolder, string providedArchiveFile)
        {
            Download(webAddress, locationFolder, providedArchiveFile, DefaultWeb_File_Size);
        }

        /// <summary>
        /// Initiates a download with a specified web address, local folder, archive file, and expected file size.
        /// </summary>
        /// <param name="webAddress">The URL of the file to download.</param>
        /// <param name="locationFolder">The local folder to save the file.</param>
        /// <param name="providedArchiveFile">The provided archive file path.</param>
        /// <param name="providedFile_Size">The expected size of the file to download.</param>
        public void Download(string webAddress, string locationFolder, string providedArchiveFile, long providedFile_Size)
        {
            Download(webAddress, locationFolder, providedArchiveFile, providedFile_Size, File_Name);
        }

        /// <summary>
        /// Initiates a download with all specified parameters.
        /// </summary>
        /// <param name="webAddress">The URL of the file to download.</param>
        /// <param name="locationFolder">The local folder to save the file.</param>
        /// <param name="providedArchiveFile">The provided archive file path.</param>
        /// <param name="providedFile_Size">The expected size of the file to download.</param>
        /// <param name="providedFile_Name">The local name for the downloaded file.</param>
        public void Download(string webAddress, string locationFolder, string providedArchiveFile, long providedFile_Size, string providedFile_Name)
        {
            Download(webAddress, locationFolder, providedArchiveFile, providedFile_Size, providedFile_Name, 0);
        }

        /// <summary>
        /// Initiates a download operation with retry logic.
        /// </summary>
        /// <param name="webAddress">The URL of the file to download.</param>
        /// <param name="locationFolder">The local folder to save the file.</param>
        /// <param name="providedArchiveFile">The provided archive file path.</param>
        /// <param name="providedFile_Size">The expected size of the file to download.</param>
        /// <param name="providedFile_Name">The local name for the downloaded file.</param>
        /// <param name="errorRate">Current retry attempt count.</param>
        /// <exception cref="Downloaded_File_Hash_Invalid_Exception">Thrown if the downloaded file's hash does not match the expected hash.</exception>
        /// <exception cref="Download_Client_Exception">Thrown if the download client is interrupted or too many retries occur.</exception>
        /// <exception cref="ArgumentException">Thrown if the URL is malformed.</exception>
        public void Download(string webAddress, string locationFolder, string providedArchiveFile, long providedFile_Size, string providedFile_Name, int errorRate)
        {
            FileStream? liveWriter = null;
            try
            {
                // Set paths and file size
                Folder_Path = !string.IsNullOrWhiteSpace(providedArchiveFile) && File.Exists(providedArchiveFile)
                    ? Path.GetDirectoryName(providedArchiveFile)! // ! used as Path.GetDirectoryName will return null if no directory exists. Assuming it will always have one from ProvidedArchiveFile if it exists
                    : Path.Combine(locationFolder, ".Launcher", "Downloads");

                File_Name = !string.IsNullOrWhiteSpace(providedFile_Name) ? providedFile_Name : Path.GetFileName(webAddress);
                File_Path = !string.IsNullOrWhiteSpace(providedArchiveFile) && File.Exists(providedArchiveFile)
                    ? providedArchiveFile
                    : Path.Combine(Folder_Path, File_Name);

                if (providedFile_Size > 0)
                {
                    Web_File_Size = providedFile_Size;
                }

                // Ensure directories exist
                EnsureDirectoryExists(locationFolder);
                EnsureDirectoryExists(Folder_Path);

                // Initialize local file state
                InitializeLocalFileState();

                if (File_Size == Web_File_Size)
                {
                    HandleCompletedDownload(errorRate);
                }
                else
                {
                    PerformDownload(webAddress, locationFolder, providedArchiveFile, providedFile_Size, providedFile_Name, errorRate);
                }
            }
#if !DEBUG
            catch (WebException ex)
            {
                Exception_Router(true, ex, true);
            }
            catch (UriFormatException ex)
            {
                Exception_Router(true, new ArgumentException(
                    $"Could not parse the URL \"{webAddress}\" - it's either malformed or is an unknown protocol.", ex));
            }
            catch (Exception ex)
            {
                Exception_Router(true, ex);
            }
#endif
            finally
            {
                liveWriter?.Flush();
                liveWriter?.Close();
                liveWriter?.Dispose();
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void InitializeLocalFileState()
        {
            if (!File.Exists(File_Path))
            {
                // Create the file if it doesn't exist. Using FileStream for proper disposal.
                using (File.Create(File_Path)) { }
                File_Size = 0;
            }
            else
            {
                File_Size = new FileInfo(File_Path).Length;

                if (File_Size > Web_File_Size)
                {
                    File.Delete(File_Path);
                    using (File.Create(File_Path)) { }
                    File_Size = 0;
                }
                else if (File_Size == Web_File_Size && File_Removal)
                {
                    if (!string.IsNullOrEmpty(File_Hash) && Hashes.Hash_SHA(File_Path) != File_Hash)
                    {
                        File.Delete(File_Path);
                        using (File.Create(File_Path)) { }
                        File_Size = 0;
                    }
                }
            }
        }

        private void HandleCompletedDownload(int errorRate)
        {
            Live_Progress?.Invoke(this,
                new Download_Data_Progress_EventArgs(Web_File_Size, File_Size, Web_File_Size_Remaining, Start_Time, errorRate));

            string calculatedLocalFile_Hash = Hashes.Hash_SHA(File_Path);

            if (calculatedLocalFile_Hash == File_Hash)
            {
                if (Complete != null && !Cancel)
                {
                    Complete(this, new Download_Data_Complete_EventArgs(true, File_Path, DateTime.Now));
                }
                else
                {
                    Cancel = true;
                }
            }
            else
            {
                throw new Downloaded_File_Hash_Invalid_Exception($"Local File does not match Provided Hash. Excepted: {File_Hash} File: {(string.IsNullOrWhiteSpace(calculatedLocalFile_Hash) ? "Null String" : calculatedLocalFile_Hash)}");
            }
        }

        private void PerformDownload(string webAddress, string locationFolder, string providedArchiveFile, long providedFile_Size, string providedFile_Name, int errorRate)
        {
            Start_Time = DateTime.Now; // Set time when request starts

            HttpWebRequest liveRequest;

            for (int attempt = errorRate; attempt <= Download_Retry_Attempts; attempt++)
            {
                if (Cancel)
                {
                    break;
                }

                try
                {
#if NETFRAMEWORK
                    liveRequest = (HttpWebRequest)WebRequest.Create(webAddress);
#else
                    // Using WebRequest.Create for compatibility, but HttpClient is preferred in modern .NET.
                    // This warning is suppressed as HttpWebRequest is used due to .NET Framework 4.6.1 target.
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    liveRequest = (HttpWebRequest)WebRequest.Create(webAddress);
#pragma warning restore SYSLIB0014
#endif
                    liveRequest.UserAgent = Download_Settings.Header;
                    liveRequest.Headers["X-UserAgent"] = Download_Settings.Header;

                    if (File_Size > 0 && providedFile_Size > 0)
                    {
                        liveRequest.AddRange(File_Size, providedFile_Size);
                    }

                    liveRequest.Timeout = Download_Settings.Launcher_WebCall_Timeout();

                    byte[] liveBuffer = new byte[Download_Block_Size];
                    File_Size_Live = File_Size;

                    using (_liveResponse = (HttpWebResponse)liveRequest.GetResponse())
                    {
                        if (_liveResponse == null)
                        {
                            throw new Exception($"Could not download \"{webAddress}\" - Received Response is Null");
                        }
                        else if (_liveResponse.ContentType?.Contains("text/html") == true)
                        {
                            throw new Exception($"Could not download \"{webAddress}\" - A web page was returned from the web server.");
                        }
                        else if (_liveResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new Exception($"Could not download \"{webAddress}\" - File not Found on web server.");
                        }
                        else
                        {
                            if (_liveResponse.StatusCode != HttpStatusCode.PartialContent && File_Size != 0)
                            {
                                // If not partial content and file size is not zero, restart download
                                File.Delete(File_Path);
                                using (File.Create(File_Path)) { }
                                File_Size = 0;
                                File_Size_Live = 0;
                            }

                            using (Stream liveStream = _liveResponse.GetResponseStream())
                            using (FileStream liveWriter = new FileStream(File_Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {
                                Web_File_Size_Remaining = _liveResponse.ContentLength;
                                int bytesRead;
                                while ((bytesRead = liveStream.Read(liveBuffer, 0, Download_Block_Size)) > 0)
                                {
                                    if (Cancel)
                                    {
                                        break;
                                    }

                                    liveWriter.Write(liveBuffer, 0, bytesRead);
                                    File_Size_Live += bytesRead;

                                    Live_Progress?.Invoke(this,
                                        new Download_Data_Progress_EventArgs(Web_File_Size, File_Size_Live, Web_File_Size_Remaining, Start_Time, attempt));

                                    if (!Disable_Download_Status_Information)
                                    {
                                        Download_Status_Information = new Download_Information()
                                        {
                                            File_Size_Total = Web_File_Size,
                                            File_Size_Current = File_Size_Live,
                                            File_Size_Remaining = Web_File_Size_Remaining,
                                            Download_Percentage = (int)(((double)File_Size_Live / Web_File_Size) * 100), // Fixed integer division
                                            Start_Time = Start_Time,
                                            Download_Attempts = attempt
                                        };
                                    }
                                }

                                if (Cancel)
                                {
                                    break;
                                }

                                // Final status update if not cancelled
                                if (!Disable_Download_Status_Information )
                                {
                                    Download_Status_Information = new Download_Information()
                                    {
                                        File_Size_Total = Web_File_Size,
                                        File_Size_Current = File_Size_Live,
                                        File_Size_Remaining = Web_File_Size_Remaining,
                                        Download_Percentage = (int)(((double)File_Size_Live / Web_File_Size) * 100), // Fixed integer division
                                        Start_Time = Start_Time,
                                        End_Time = DateTime.Now,
                                        Download_Complete = true,
                                        Download_Attempts = attempt
                                    };
                                }

                                // Check completion and hash
                                if (File_Size_Live == Web_File_Size)
                                {
                                    string calculatedLocalFile_Hash = Hashes.Hash_SHA(File_Path);

                                    if (calculatedLocalFile_Hash == File_Hash)
                                    {
                                        Complete?.Invoke(this, new Download_Data_Complete_EventArgs(true, File_Path, DateTime.Now));
                                        return; // Successfully completed, exit loop and method
                                    }
                                    else
                                    {
                                        // Hash mismatch, attempt retry if within limits
                                        if (attempt < Download_Retry_Attempts)
                                        {
                                            File.Delete(File_Path); // Delete corrupted file for retry
                                            using (File.Create(File_Path)) { } // Recreate empty file
                                            File_Size = 0;
                                            File_Size_Live = 0;
                                            // Continue to next iteration (retry)
                                        }
                                        else
                                        {
                                            throw new Downloaded_File_Hash_Invalid_Exception($"Local File does not match Provided Hash. Expected: {File_Hash} File: {(string.IsNullOrWhiteSpace(calculatedLocalFile_Hash) ? "Null String" : calculatedLocalFile_Hash)}");
                                        }
                                    }
                                }
                                else if (File_Size_Live < Web_File_Size)
                                {
                                    // Incomplete download, attempt retry if within limits
                                    if (attempt < Download_Retry_Attempts)
                                    {
                                        // Continue to next iteration (retry)
                                    }
                                    else
                                    {
                                        throw new Download_Client_Exception("Download Client failed to download the complete file after multiple attempts. Please Manually Retry.");
                                    }
                                }
                                else if (File_Size_Live > Web_File_Size)
                                {
                                    // Downloaded more than expected, indicating an issue. Delete and retry.
                                    if (attempt < Download_Retry_Attempts)
                                    {
                                        File.Delete(File_Path);
                                        using (File.Create(File_Path)) { }
                                        File_Size = 0;
                                        File_Size_Live = 0;
                                        // Continue to next iteration (retry)
                                    }
                                    else
                                    {
                                        throw new Download_Client_Exception("Download Client downloaded more data than expected after multiple attempts. Please Manually Retry.");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Exception_Router(true, ex, true);
                    if (attempt < Download_Retry_Attempts)
                    {
                        // Log retry attempt and continue
                        Thread.Sleep(1000 * (attempt + 1)); // Exponential back-off for retries
                    }
                    else
                    {
                        throw; // Re-throw if out of retries
                    }
                }
                catch (UriFormatException ex)
                {
                    Exception_Router(true, new ArgumentException(
                        $"Could not parse the URL \"{webAddress}\" - it's either malformed or is an unknown protocol.", ex));
                    return; // Fatal error, no retry
                }
                catch (Exception ex)
                {
                    Exception_Router(true, ex);
                    if (attempt < Download_Retry_Attempts)
                    {
                        // Log retry attempt and continue
                        Thread.Sleep(1000 * (attempt + 1)); // Exponential back-off for retries
                    }
                    else
                    {
                        throw; // Re-throw if out of retries
                    }
                }
            }

            if (!Cancel && File_Size_Live != Web_File_Size)
            {
                throw new Download_Client_Exception("Download Client was being Interrupted or did not complete. Please Manually Retry.");
            }
        }

        /// <summary>
        /// Download a file from a list of URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        /// <param name="webAddressList">List of URLs to try downloading from.</param>
        public void Download(List<string> webAddressList)
        {
            Download(webAddressList, string.Empty, -1);
        }

        /// <summary>
        /// Download a file from a list of URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        /// <param name="webAddressList">List of URLs to try downloading from.</param>
        /// <param name="locationFolder">The local folder to save the file.</param>
        /// <param name="providedFile_Size">The expected size of the file to download.</param>
        /// <param name="providedArchiveFile">The provided archive file path.</param>
        public void Download(List<string> webAddressList, string locationFolder, long providedFile_Size, string providedArchiveFile = "")
        {
            if (webAddressList == null)
            {
                Exception_Router(true, new ArgumentNullException(nameof(webAddressList)));
                return;
            }
            if (webAddressList.Count == 0)
            {
                Exception_Router(true, new ArgumentException("Web address list is empty.", nameof(webAddressList)));
                return;
            }

            Exception? lastException = null;
            foreach (string singleWebAddress in webAddressList)
            {
                try
                {
                    Download(singleWebAddress, locationFolder, providedArchiveFile, providedFile_Size);
                    lastException = null; // Successfully downloaded
                    break;
                }
                catch (Exception e)
                {
                    lastException = e;
                    // Log the attempt failure if needed, then try the next URL
                }
            }

            if (lastException != null)
            {
                Exception_Router(true, lastException);
            }
        }

#if !NETFRAMEWORK
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#endif
        /// <summary>
        /// Asynchronously download a file from the url.
        /// </summary>
        public void AsyncDownload(string webAddress)
        {
            ThreadPool.QueueUserWorkItem(WaitCallbackMethod, new string[] { webAddress, string.Empty });
        }

        /// <summary>
        /// Asynchronously download a file from the url to the destination folder.
        /// </summary>
        public void AsyncDownload(string webAddress, string locationFolder)
        {
            ThreadPool.QueueUserWorkItem(WaitCallbackMethod, new string[] { webAddress, locationFolder });
        }

        /// <summary>
        /// Asynchronously download a file from a list of URLs.
        /// </summary>
        public void AsyncDownload(List<string> webAddressList, string locationFolder)
        {
            ThreadPool.QueueUserWorkItem(WaitCallbackMethod, new object[] { webAddressList, locationFolder });
        }

        /// <summary>
        /// Asynchronously download a file from a list of URLs.
        /// </summary>
        public void AsyncDownload(List<string> webAddressList)
        {
            ThreadPool.QueueUserWorkItem(WaitCallbackMethod, new object[] { webAddressList, string.Empty });
        }
#if !NETFRAMEWORK
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#endif
        /// <summary>
        /// A WaitCallback used by the AsyncDownload methods.
        /// </summary>
        private void WaitCallbackMethod(object? objectData)
        {
            if (objectData == null) return;

            if (objectData is string[] stringArray)
            {
                if (stringArray.Length >= 2) // Expect at least webAddress and locationFolder
                {
                    string webAddress = stringArray[0];
                    string locationFolder = stringArray[1];
                    string providedArchiveFile = stringArray.Length > 2 ? stringArray[2] : string.Empty;
                    long providedFile_Size = stringArray.Length > 3 && long.TryParse(stringArray[3], out long parsedSize) ? parsedSize : -1;
                    string providedFile_Name = stringArray.Length > 4 ? stringArray[4] : string.Empty;

                    Download(webAddress, locationFolder, providedArchiveFile, providedFile_Size, providedFile_Name);
                }
            }
            else if (objectData is object[] objectArray)
            {
                if (objectArray.Length >= 2) // Expect at least Web_Address_List and Location_Folder
                {
                    if (objectArray[0] is List<string> webAddressList)
                    {
                        string? locationFolder = objectArray[1] as string;
                        long providedFile_Size = objectArray.Length > 2 && objectArray[2] is string sizeString && long.TryParse(sizeString, out long parsedSize) ? parsedSize : -1;
                        string providedArchiveFile = objectArray.Length > 3 && objectArray[3] is string archiveString ? archiveString : string.Empty;

                        if (!string.IsNullOrWhiteSpace(locationFolder))
                        {
                            Download(webAddressList, locationFolder, providedFile_Size, providedArchiveFile);
                        }
                        else
                        {
                            // Handle case where locationFolder is null or whitespace for List<string> overload
                            Download(webAddressList, AppDomain.CurrentDomain.BaseDirectory, providedFile_Size, providedArchiveFile);
                        }
                    }
                }
            }
        }
    }
}