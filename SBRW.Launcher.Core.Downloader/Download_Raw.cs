#if !TEST
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Cache;
using System.Net;
using System.Text;
using System.Threading;
using SBRW.Launcher.Core.Downloader.Exception_;
using SBRW.Launcher.Core.Downloader.Web_;
using System.Linq;
using SBRW.Launcher.Core.Downloader.Extension_;
using SBRW.Launcher.Core.Downloader.EventArg_;
using System.Net.Http;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Raw
    {
        /// <summary>
        /// 
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// 
            /// </summary>
            Queue,
            /// <summary>
            /// 
            /// </summary>
            Downloading,
            /// <summary>
            /// 
            /// </summary>
            Downloaded,
            /// <summary>
            /// 
            /// </summary>
            Cancel,
            /// <summary>
            /// 
            /// </summary>
            Stopped,
            /// <summary>
            /// 
            /// </summary>
            Unknown
        }
        /// <summary>
        /// 
        /// </summary>
        private static ManualResetEvent Live_Worker = new ManualResetEvent(false);
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Mode Live_Mode { get; set; } = Mode.Unknown;
        /// <summary>
        /// 
        /// </summary>
        public string Folder_Path { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game Files");
        /// <summary>
        /// 
        /// </summary>
        public string Server_URL { get; set; } = "http://localhost";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Data_Exception_Handler(object Sender, Download_Exception_EventArgs Events);
        /// <summary>
        /// 
        /// </summary>
        public event Download_Data_Exception_Handler? Internal_Error;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Data_Progress_Handler(object Sender, Download_Data_Progress_EventArgs Events);
        /// <summary>
        /// 
        /// </summary>
        public event Download_Data_Progress_Handler? Live_Progress;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Data_Completion_Handler(object Sender, Download_Data_Complete_EventArgs Events);
        /// <summary>
        /// 
        /// </summary>
        public event Download_Data_Completion_Handler? Complete;
        /// <summary>
        /// 
        /// </summary>
        public Download_Information? Download_Status_Information { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Download_Information? Download_Status() { return Download_Status_Information; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Event_Hook"></param>
        /// <param name="Exception_Caught"></param>
        internal void Exception_Router(bool Event_Hook, Exception Exception_Caught)
        {
            Exception_Router(Event_Hook, Exception_Caught, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Event_Hook"></param>
        /// <param name="Exception_Caught"></param>
        /// <param name="Related_To_WebClient"></param>
        internal void Exception_Router(bool Event_Hook, Exception Exception_Caught, bool Related_To_WebClient)
        {
            if (this.Internal_Error != null && Event_Hook)
            {
                this.Internal_Error(this, new Download_Exception_EventArgs(Exception_Caught, DateTime.Now, Related_To_WebClient));
            }
            else
            {
                throw Exception_Caught;
            }
        }
        public BackgroundWorker Start()
        {
            BackgroundWorker Creator = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            Creator.DoWork += Start_File_Scan;
            Creator.RunWorkerCompleted += Start_File_Scan_Completed;

            return Creator;
        }

        public int Removal(List<Scanned_Files> Generated_Scanned_List)
        {
            if (Generated_Scanned_List == default)
            {
                return -1;
            }
            else if (Generated_Scanned_List.Count <= 0)
            {
                return -1;
            }
            else
            {
                int Return_Value = 0;
                foreach (Scanned_Files Bad_File_OR_Folder in Generated_Scanned_List)
                {
                    if (Bad_File_OR_Folder != default)
                    {
                        if (!Bad_File_OR_Folder.Skip)
                        {
                            /* FileSystemInfo can be either a File or Directory, so do two Checks to ensure we know what it is */
                            if (Bad_File_OR_Folder.File_System_Info != default)
                            {
                                try
                                {
                                    if (Directory.Exists(Bad_File_OR_Folder.File_System_Info.FullName))
                                    {
                                        Directory.Delete(Bad_File_OR_Folder.File_System_Info.FullName, true);
                                        Log_Verify.Deleted("Folder - [FSI]: " + Bad_File_OR_Folder.File_System_Info.Name);
                                    }
                                    else if (File.Exists(Bad_File_OR_Folder.File_System_Info.FullName))
                                    {
                                        File.Delete(Bad_File_OR_Folder.File_System_Info.FullName);
                                        Log_Verify.Deleted("File - [FSI]: " + Bad_File_OR_Folder.File_System_Info.Name);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Files_Deletion_Error_Total++;
                                    Exception_Router(true, Error);
                                }
                            }
                            /* Detected as a File */
                            if (Bad_File_OR_Folder.File_Info != default)
                            {
                                try
                                {
                                    if (File.Exists(Bad_File_OR_Folder.File_Info.FullName))
                                    {
                                        Bad_File_OR_Folder.File_Info.Delete();
                                        Log_Verify.Deleted("File: " + Bad_File_OR_Folder.File_Info.Name);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Files_Deletion_Error_Total++;
                                    Exception_Router(true, Error);
                                }
                            }
                            /* Detected as a Directory */
                            if (Bad_File_OR_Folder.Directory_Info != default)
                            {
                                try
                                {
                                    if (Directory.Exists(Bad_File_OR_Folder.Directory_Info.FullName))
                                    {
                                        Directory.Delete(Bad_File_OR_Folder.Directory_Info.FullName, true);
                                        Log_Verify.Deleted("Folder: " + Bad_File_OR_Folder.Directory_Info.Name);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Files_Deletion_Error_Total++;
                                    Exception_Router(true, Error);
                                }
                            }
                        }

                        Generated_Scanned_List.Remove(Bad_File_OR_Folder);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// TODO: UPDATE TEXT FOR TRANSLATIONS
        private void File_Scan_()
        {
            if (!string.IsNullOrWhiteSpace(Folder_Path))
            {
                //Log.Info("VERIFY HASH: Checking and Deleting '.orig' Files and Symbolic Folders");
                //Label_Verify_Scan.SafeInvokeAction(() => Label_Verify_Scan.Text = "Removing any '.orig' Files in Game Directory");

                DirectoryInfo Game_Files_Directory = new DirectoryInfo(Folder_Path);
                /* */
                if (Game_Files_Directory.Exists)
                {
                    /* */
                    foreach (FileInfo Matched_File in Game_Files_Directory.EnumerateFiles("*.orig", SearchOption.AllDirectories))
                    {
                        if (!Live_Events.Cancel)
                        {
                            Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { File_Info = Matched_File });
                        }
                        else
                        {
                            break;
                        }
                    }
                    /* */
                    foreach (DirectoryInfo Found_Directory in Game_Files_Directory.EnumerateDirectories())
                    {
                        if (!Live_Events.Cancel)
                        {
                            if (ModNetHandler.IsSymbolic(Found_Directory.FullName))
                            {
                                if (Directory.Exists(Found_Directory.FullName))
                                {
                                    Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { Directory_Info = Found_Directory });
                                }
                                else if (File.Exists(Found_Directory.FullName))
                                {
                                    Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { File_System_Info = Found_Directory });
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    /* */
                    foreach (DirectoryInfo Found_Directory in Game_Files_Directory.GetDirectories())
                    {
                        if (!Live_Events.Cancel)
                        {
                            foreach (FileInfo Matched_File in Found_Directory.EnumerateFiles("*.orig", SearchOption.AllDirectories))
                            {
                                if (!Live_Events.Cancel)
                                {
                                    Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { File_Info = Matched_File });
                                }
                                else
                                {
                                    break;
                                }
                            }
                            /* */
                            foreach (DirectoryInfo Found_Directories in Found_Directory.EnumerateDirectories())
                            {
                                if (!Live_Events.Cancel)
                                {
                                    if (ModNetHandler.IsSymbolic(Found_Directories.FullName))
                                    {
                                        if (Directory.Exists(Found_Directories.FullName))
                                        {
                                            Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { Directory_Info = Found_Directories });
                                        }
                                        else if (File.Exists(Found_Directories.FullName))
                                        {
                                            Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { File_System_Info = Found_Directories });
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    /* */
                    DirectoryInfo Scripts_Folder = new DirectoryInfo(Path.Combine(Save_Settings.Live_Data.Game_Path, "scripts"));
                    /* */
                    if (Scripts_Folder.Exists)
                    {
                        foreach (FileInfo Scripts_files in Scripts_Folder.GetFiles())
                        {
                            if (!Live_Events.Cancel)
                            {
                                Generated_Scanned_List.Add(new Json_List_Scanned_Game_Files() { File_Info = Scripts_files, Skip = Skip_Scripts_Folder });
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="Live_Events"></param>
        private void Start_File_Scan_Completed(object _, RunWorkerCompletedEventArgs Live_Events)
        {
            if (!Live_Events.Cancelled)
            {
                Verify_Hash_Status = Raw_Download_Progress.Removing;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Start_Removal(object _, DoWorkEventArgs Live_Events)
        {
            if (!Live_Events.Cancel)
            {
                Verify_Hash_Status = Raw_Download_Progress.Removing;
                foreach (Json_List_Scanned_Game_Files Bad_File_OR_Folder in Generated_Scanned_List)
                {
                    if (Bad_File_OR_Folder != default)
                    {
                        if (!Bad_File_OR_Folder.Skip)
                        {
                            /* FileSystemInfo can be either a File or Directory, so do two Checks to ensure we know what it is */
                            if (Bad_File_OR_Folder.File_System_Info != default)
                            {
                                try
                                {
                                    if (Directory.Exists(Bad_File_OR_Folder.File_System_Info.FullName))
                                    {
                                        Directory.Delete(Bad_File_OR_Folder.File_System_Info.FullName, true);
                                        Log_Verify.Deleted("Folder - [FSI]: " + Bad_File_OR_Folder.File_System_Info.Name);
                                    }
                                    else if (File.Exists(Bad_File_OR_Folder.File_System_Info.FullName))
                                    {
                                        File.Delete(Bad_File_OR_Folder.File_System_Info.FullName);
                                        Log_Verify.Deleted("File - [FSI]: " + Bad_File_OR_Folder.File_System_Info.Name);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Files_Deletion_Error_Total++;
                                    Log_Verify.Error("FSI: " + Bad_File_OR_Folder.File_System_Info.Name + " Error: " + Error.Message);
                                    Log_Verify.ErrorIC("FSI: " + Bad_File_OR_Folder.File_System_Info.Name + " Error: " + Error.HResult);
                                    Log_Verify.ErrorFR("FSI: " + Bad_File_OR_Folder.File_System_Info.Name + " Error: " + Error.ToString());
                                }
                            }
                            /* Detected as a File */
                            if (Bad_File_OR_Folder.File_Info != default)
                            {
                                try
                                {
                                    if (File.Exists(Bad_File_OR_Folder.File_Info.FullName))
                                    {
                                        Bad_File_OR_Folder.File_Info.Delete();
                                        Log_Verify.Deleted("File: " + Bad_File_OR_Folder.File_Info.Name);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Files_Deletion_Error_Total++;
                                    Log_Verify.Error("File: " + Bad_File_OR_Folder.File_Info.Name + " Error: " + Error.Message);
                                    Log_Verify.ErrorIC("File: " + Bad_File_OR_Folder.File_Info.Name + " Error: " + Error.HResult);
                                    Log_Verify.ErrorFR("File: " + Bad_File_OR_Folder.File_Info.Name + " Error: " + Error.ToString());
                                }
                            }
                            /* Detected as a Directory */
                            if (Bad_File_OR_Folder.Directory_Info != default)
                            {
                                try
                                {
                                    if (Directory.Exists(Bad_File_OR_Folder.Directory_Info.FullName))
                                    {
                                        Directory.Delete(Bad_File_OR_Folder.Directory_Info.FullName, true);
                                        Log_Verify.Deleted("Folder: " + Bad_File_OR_Folder.Directory_Info.Name);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Files_Deletion_Error_Total++;
                                    Log_Verify.Error("Folder: " + Bad_File_OR_Folder.Directory_Info.Name + " Error: " + Error.Message);
                                    Log_Verify.ErrorIC("Folder: " + Bad_File_OR_Folder.Directory_Info.Name + " Error: " + Error.HResult);
                                    Log_Verify.ErrorFR("Folder: " + Bad_File_OR_Folder.Directory_Info.Name + " Error: " + Error.ToString());
                                }
                            }
                        }

                        Generated_Scanned_List.Remove(Bad_File_OR_Folder);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="Live_Events"></param>
        private void Start_Removal_Completed(object _, RunWorkerCompletedEventArgs Live_Events)
        {
            if (!Live_Events.Cancelled)
            {
                Verify_Hash_Status = Raw_Download_Progress.Checksums_File;
            }
            else
            {
                Verify_Hash_Status = Raw_Download_Progress.Stopped;
            }
        }
        private void Start_Checksums_Download(object _, DoWorkEventArgs Live_Events)
        {
            if (!Live_Events.Cancel)
            {
                bool CheckSums_File_Found = "checksums.dat".Hash_SHA() == "80D272597981DABA49F5022BBF36FF302FC9D13E";

                if (CheckSums_File_Found)
                {
                    Verify_Hash_Status = Raw_Download_Progress.Checksums_File_Found;
                    /* Read Local checksums.dat */
                    File_Checksum = File.ReadAllLines("checksums.dat");
                }
                else
                {
                    /* Fetch and Read Remote checksums.dat */
                    //Label_Verify_Scan.SafeInvokeAction(() => Label_Verify_Scan.Text = "Downloading Checksums File");

                    Uri URLCall = new Uri(Verify_CDN_URL + "/unpacked/checksums.dat");
                    ServicePointManager.FindServicePoint(URLCall).ConnectionLeaseTimeout = (int)TimeSpan.FromSeconds(Launcher_Value.Launcher_WebCall_Timeout_Enable ?
                                Launcher_Value.Launcher_WebCall_Timeout() : 60).TotalMilliseconds;
                    var Client = new WebClient
                    {
                        Encoding = Encoding.UTF8,
                        CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
                    };
                    if (!Launcher_Value.Launcher_Alternative_Webcalls())
                    {
                        Client = new WebClientWithTimeout { Encoding = Encoding.UTF8, CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore) };
                    }
                    else
                    {
                        Client.Headers.Add("user-agent", "SBRW Launcher " +
                        Application.ProductVersion + " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                    }

                    bool ErrorFree = true;

                    try
                    {
                        File_Checksum = Client.DownloadString(URLCall).Split('\n');
                    }
                    catch (Exception Error)
                    {
                        LogToFileAddons.OpenLog("VERIFY HASH CHECKSUMS", "Downloading of the Checksums File has Encountered an Error", Error, "Error", false);
                        ErrorFree = false;
                    }
                    finally
                    {
                        Client?.Dispose();
                    }

                    if (ErrorFree)
                    {
                        File.WriteAllLines("checksums.dat", File_Checksum);
                    }
                    else
                    {
                        Verify_Hash_Status = Raw_Download_Progress.Checksums_File_Error;
                    }
                }

                if (CheckSums_File_Found)
                {
                    /* We need to Verify that the CDN Supports Verify or Raw Download - DavidCarbon */
                    using (HttpClient Alpha_Client = new HttpClient())
                    {
                        try
                        {
                            Alpha_Client.Timeout = TimeSpan.FromSeconds(30);
                            HttpRequestMessage Client_Request = new HttpRequestMessage(HttpMethod.Head, Verify_CDN_URL + "/unpacked/checksums.dat");
                            HttpResponseMessage Client_Response = Alpha_Client.SendAsync(Client_Request).GetAwaiter().GetResult();
                            Verify_Hash_Status = Client_Response.IsSuccessStatusCode ? Raw_Download_Progress.Verifying : Raw_Download_Progress.Checksums_Not_Available;
                        }
                        catch
                        {
                            Verify_Hash_Status = Raw_Download_Progress.Checksums_Not_Available;
                        }
                    }
                }
            }
        }
        private void Start_Checksums_Completed(object _, RunWorkerCompletedEventArgs Live_Events)
        {
            if (!Live_Events.Cancelled)
            {
                Stage_Process = 3;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="Live_Events"></param>
        private void Start_File_Download(object _, DoWorkEventArgs Live_Events)
        {
            if (!Live_Events.Cancel)
            {
                Presence_Launcher.Status(26);
                /* START Show Redownloader Progress
                StartScanner.SafeInvokeAction(() => StartScanner.Visible = false);
                StopScanner.SafeInvokeAction(() => StopScanner.Visible = true);
                

                if (!Screen_Instance.DisposedForm())
                {
                    Screen_Instance.Label_Verify_Scan.Text = "Currently (re)downloading files. This part may take awhile depending on your connection.";
                }*/

                if (Generated_Scanned_Invalid_List.Any())
                {
                    //DownloadProgressText.SafeInvokeAction(() => DownloadProgressText.Text = "\nPreparing to Download Files");

                    Files_Total = Generated_Scanned_Invalid_List.Count;

                    foreach (Json_List_Invalid_Game_Files Found_File_Invaild in Generated_Scanned_Invalid_List)
                    {
                        if (!Live_Events.Cancel)
                        {
                            try
                            {
                                while (File_Downloading)
                                {
                                    if (Live_Events.Cancel)
                                    {
                                        break;
                                    }
                                }

                                if (!Live_Events.Cancel)
                                {
                                    Uri URLCall = new Uri(Found_File_Invaild.Download_Url);
                                    int Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

                                    if (Found_File_Invaild.Download_Url.Contains("copspeechdat"))
                                    {
                                        Timeout = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
                                    }
                                    else if (Found_File_Invaild.Download_Url.Contains("nfs09mx.mus"))
                                    {
                                        Timeout = (int)TimeSpan.FromMinutes(15).TotalMilliseconds;
                                    }

                                    ServicePointManager.FindServicePoint(URLCall).ConnectionLeaseTimeout = Timeout;

                                    var Client = new WebClient()
                                    {
                                        Encoding = Encoding.UTF8,
                                        CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
                                    };
                                    if (!Launcher_Value.Launcher_Alternative_Webcalls())
                                    {
                                        Client = new WebClientWithTimeout()
                                        {
                                            Encoding = Encoding.UTF8,
                                            CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
                                        };
                                    }
                                    else
                                    {
                                        Client.Headers.Add("user-agent", "SBRW Launcher " +
                                        Application.ProductVersion + " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                                    }

                                    Client.DownloadProgressChanged += (Systems, RecevingData) =>
                                    {
                                        if (RecevingData.TotalBytesToReceive >= 1 && !Live_Events.Cancel)
                                        {
                                            /*
                                            if (Screen_Instance != default)
                                            {
                                                if (!(Screen_Instance.Disposing || Screen_Instance.IsDisposed))
                                                {
                                                    Screen_Instance.Label_Verify_Scan.Text = "Currently (re)downloading files. This part may take awhile depending on your connection.";
                                                }
                                            }
                                            
                                            TextBox_Verify_Scan
                                            DownloadProgressText.SafeInvokeAction(() =>
                                            DownloadProgressText.Text = "Downloading File [ " + RedownloadedCount + " / " +
                                            CurrentCount + " ]:\n" + CurrentDownloadingFile + "\n" + Time_Conversion.FormatFileSize(RecevingData.BytesReceived) +
                                            " of " + Time_Conversion.FormatFileSize(RecevingData.TotalBytesToReceive));
                                            */
                                        }
                                        else if (Live_Events.Cancel)
                                        {
                                            Client.CancelAsync();
                                        }
                                    };
                                    Client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);

                                    try
                                    {
                                        Client.DownloadFileAsync(URLCall, Found_File_Invaild.Path_Full);
                                        File_Downloading = true;
                                    }
                                    catch (Exception Error)
                                    {
                                        if (!Live_Events.Cancel)
                                        {
                                            Files_Download_Error_Total++;
                                            File_Downloading = false;
                                        }

                                        LogToFileAddons.OpenLog("VERIFY HASH", string.Empty, Error, string.Empty, true);
                                    }
                                    finally
                                    {
                                        Client?.Dispose();
                                    }

                                    Generated_Scanned_Invalid_List.Remove(Found_File_Invaild);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception Error)
                            {
                                if (!Live_Events.Cancel)
                                {
                                    Files_Download_Error_Total++;
                                }

                                LogToFileAddons.OpenLog("VERIFY HASH", string.Empty, Error, string.Empty, true);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Start_File_Scan_Invalid()
        {
            if (!Live_Events.Cancel)
            {
                try
                {
                    FunctionStatus.IsVerifyHashDisabled = true;



                    Files_Total = getFilesToCheck.Length;
                    Files_Scanned_Total = 0;

                    for (var i = 0; i < Files_Total; i++)
                    {
                        if (!Live_Events.Cancel)
                        {
                            Generated_Scanned_Invalid_List.Add(new Json_List_Invalid_Game_Files()
                            {
                                Hash = getFilesToCheck[i].Split(' ')[0].Trim(),
                                Path_Truncated = getFilesToCheck[i].Split(' ')[1].Trim(),
                                Path_Full = Path.Combine(Save_Settings.Live_Data.Game_Path + getFilesToCheck[i].Split(' ')[1].Trim()),
                                Download_Url = Verify_CDN_URL + "/unpacked" + getFilesToCheck[i].Split(' ')[1].Trim().Replace("\\", "/")
                            });
                        }
                        else
                        {
                            break;
                        }
                    }

                    foreach (Json_List_Invalid_Game_Files Current_File_Scan in Generated_Scanned_Invalid_List)
                    {
                        if (!Live_Events.Cancel)
                        {
                            if (!File.Exists(Current_File_Scan.Path_Full))
                            {
                                Log_Verify.Missing("File: " + Current_File_Scan.Path_Truncated);
                            }
                            else
                            {
                                if (Current_File_Scan.Hash != Current_File_Scan.Path_Full.Hash_SHA().Trim())
                                {
                                    Log_Verify.Invalid("File: " + Current_File_Scan.Path_Truncated);
                                }
                                else
                                {
                                    Generated_Scanned_Invalid_List.Remove(Current_File_Scan);
                                    Log_Verify.Valid("File: " + Current_File_Scan.Path_Truncated);
                                }
                            }

                            Files_Scanned_Total++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    Log.Info("VERIFY HASH: Scan Completed");
                    if (!Generated_Scanned_Invalid_List.Any() || !Live_Events.Cancel)
                    {
                        Verify_Hash_Status = Raw_Download_Progress.Passed;
                    }
                    else
                    {
                        Log.Info("VERIFY HASH: Found Invalid or Missing Files and will Start File Downloader");
                        Verify_Hash_Status = Raw_Download_Progress.Removing;
                    }
                }
                catch (Exception Error)
                {
                    LogToFileAddons.OpenLog("VERIFY HASH", string.Empty, Error, string.Empty, true);
                }
            }
        }
        public void Download(string[] Live_Files_Array)
        {
            /* Set Time when we Started Request */
            if (Start_Time == default)
            {
                Start_Time = DateTime.Now;
            }

            foreach (string Single_Picked_File in Live_Files_Array)
            {
                if (!Live_Mode.Equals(Mode.Cancel))
                {
                    try
                    {
                        if (Live_Mode.Equals(Mode.Downloading))
                        {
                            Live_Worker.WaitOne();
                        }

                        if (!Live_Mode.Equals(Mode.Cancel))
                        {
                            CurrentCount = Live_Files_Array.Count();

                            string Combined_Path = Folder_Path + Single_Picked_File;
                            string Server_Address = Server_URL + "/unpacked" + Single_Picked_File.Replace("\\", "/");

                            if (File.Exists(Combined_Path))
                            {
                                try
                                {
                                    Log_Verify.Deleted("File: " + Combined_Path);
                                    File.Delete(Combined_Path);
                                }
                                catch (Exception Error)
                                {
                                    Log_Verify.Error("File: " + Combined_Path + " Error: " + Error.Message);
                                    Log_Verify.ErrorIC("File: " + Combined_Path + " Error: " + Error.HResult);
                                    Log_Verify.ErrorFR("File: " + Combined_Path + " Error: " + Error.ToString());
                                }
                            }

                            try
                            {
                                if (!string.IsNullOrWhiteSpace(Combined_Path))
                                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                                    if (!new FileInfo(Combined_Path).Directory.Exists)
                                    {
                                        new FileInfo(Combined_Path).Directory.Create();
                                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                                }
                            }
                            catch (Exception Error) 
                            {
                                Exception_Router(true, Error);
                            }

                            Uri URLCall = new Uri(Server_Address);
                            int Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

                            if (Server_Address.Contains("copspeechdat"))
                            {
                                Timeout = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
                            }
                            else if (Server_Address.Contains("nfs09mx.mus"))
                            {
                                Timeout = (int)TimeSpan.FromMinutes(15).TotalMilliseconds;
                            }

                            ServicePointManager.FindServicePoint(URLCall).ConnectionLeaseTimeout = Timeout;

                            var Client = new WebClient()
                            {
                                Encoding = Encoding.UTF8,
                                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
                            };
                            if (!Download_Settings.Alternative_WebCalls())
                            {
                                Client = new WebClientWithTimeout()
                                {
                                    Encoding = Encoding.UTF8,
                                    CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
                                };
                            }
                            else
                            {
                                Client.Headers.Add("user-agent", Download_Settings.Header);
                            }

                            Client.DownloadProgressChanged += (Systems, RecevingData) =>
                            {
                                if (Live_Mode.Equals(Mode.Cancel))
                                {
                                    Client.CancelAsync();
                                }
                                else if(Live_Mode.Equals(Mode.Downloading))
                                {
                                    if (this.Live_Progress != default)
                                    {
                                        this.Live_Progress(this,
                                            new Download_Data_Progress_EventArgs(RecevingData.TotalBytesToReceive, RecevingData.BytesReceived, RecevingData.BytesReceived - RecevingData.TotalBytesToReceive, Start_Time, 0));
                                    }

                                    Download_Status_Information = new Download_Information()
                                    {
                                        File_Size_Total = RecevingData.TotalBytesToReceive,
                                        File_Size_Current = RecevingData.BytesReceived,
                                        File_Size_Remaining = RecevingData.BytesReceived - RecevingData.TotalBytesToReceive,
                                        Download_Percentage = (int)((((double)RecevingData.BytesReceived) / RecevingData.TotalBytesToReceive) * 100),
                                        Start_Time = Start_Time,
                                        End_Time = DateTime.Now,
                                        Download_Complete = true,
                                        Download_Attempts = Error_Rate
                                    };
                                }
                            };
                            Client.DownloadFileCompleted += (Systems, RecevingData) =>
                            {
                                /*TODO: 
                                 * - Add Logger for Completed Files 
                                 * - Add Logger for Error Files
                                 */
                            };

                            try
                            {
                                Client.DownloadFileAsync(URLCall, Combined_Path);
                                Live_Mode = Mode.Downloading;
                            }
                            catch (Exception Error)
                            {
                                if (!Live_Mode.Equals(Mode.Cancel))
                                {
                                    RedownloadErrorCount++;
                                    Live_Mode = Mode.Queue;
                                }

                                LogToFileAddons.OpenLog("VERIFY HASH", string.Empty, Error, string.Empty, true);
                            }
                            finally
                            {
                                Client?.Dispose();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception Error)
                    {
                        if (!Live_Mode.Equals(Mode.Cancel))
                        {
                            RedownloadErrorCount++;
                            Live_Mode = Mode.Queue;
                        }

                        LogToFileAddons.OpenLog("VERIFY HASH", string.Empty, Error, string.Empty, true);
                    }
                }
                else
                {
                    break;
                }
            }

            if ((this.Complete != default) && !Live_Mode.Equals(Mode.Cancel))
            {
                this.Complete(this, new Download_Data_Complete_EventArgs(true, Folder_Path, DateTime.Now));
                Live_Mode = Mode.Stopped;
            }
        }
    }
}
#endif