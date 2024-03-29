﻿using SBRW.Launcher.Core.Downloader.EventArg_;
using SBRW.Launcher.Core.Downloader.Exception_;
using SBRW.Launcher.Core.Downloader.Extension_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Client
    {
        /// <summary>
        /// Read the file in chunks (in bytes)
        /// </summary>
        /// <remarks>Default is 1KB</remarks>
        public int Download_Block_Size { get; set; } = 1024;
        /// <summary>
        /// 
        /// </summary>
        public Download_Information? Download_Status_Information { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Download_Information? Download_Status() { return Download_Status_Information; }
        /// <summary>
        /// Downloader Download Attempts
        /// </summary>
        public int Download_Retry_Attempts { get; set; } = 10;
        /// <summary>
        /// 
        /// </summary>
        public bool Disable_Download_Status_Information { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// File Hash Comparison
        /// </summary>
        public string File_Hash { get; set; } = "88C886B6D131C052365C3D6D14E14F67A4E2C253";
        /// <summary>
        /// File Name to be Set Locally
        /// </summary>
        public string File_Name { get; set; } = "GameFiles.sbrwpack";
        /// <summary>
        /// File Size on Local Disk
        /// </summary>
        public long File_Size { get; set; } = 0;
        /// <summary>
        /// File Size that is being updated by Downloader
        /// </summary>
        public long File_Size_Live { get; internal set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public string Folder_Path { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game Files");
        /// <summary>
        /// 
        /// </summary>
        public string File_Path { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game Files", ".Launcher", "Downloads", "GameFiles.sbrwpack");
        /// <summary>
        /// Removes invalid File if File Hash does not Match
        /// </summary>
        public bool File_Removal { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Web_URL { get; set; } = "http://localhost";
        /// <summary>
        /// Expected File Size on the Web Server
        /// </summary>
        public long Web_File_Size { get; set; } = 3862102244;
        /// <summary>
        /// 
        /// </summary>
        public long Web_File_Size_Remaining { get; internal set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        private HttpWebResponse? Live_Response { get; set; }
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
            if (Live_Response != null)
            {
                Live_Response.Close();
                Live_Response.Dispose();
                Live_Response = null;
            }

            if (Related_To_WebClient)
            {
                /* Lets stop the download to pervert any memory leaks */
                this.Cancel = true;
            }

            if (this.Internal_Error != null && Event_Hook)
            {
                this.Internal_Error(this, new Download_Exception_EventArgs(Exception_Caught, DateTime.Now, Related_To_WebClient));
            }
            else
            {
                throw Exception_Caught;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Download()
        {
            Download(Web_URL, Folder_Path, File_Path, Web_File_Size, File_Name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        public void Download(string Web_Address)
        {
            Download(Web_Address, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        public void Download(string Web_Address, string Location_Folder)
        {
            Download(Web_Address, Location_Folder, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, 3862102244);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, "GameFiles.sbrwpack");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Provided_File_Name"></param>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_File_Name)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_File_Name, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Provided_File_Name"></param>
        /// <param name="Error_Rate"></param>
        /// <exception cref="Downloaded_File_Hash_Invalid_Exception"></exception>
        /// <exception cref="Exception"></exception>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_File_Name, int Error_Rate)
        {
            HttpWebRequest Live_Request;
            FileStream? Live_Writer = default;

            try
            {
#if !NETFRAMEWORK
#pragma warning disable CS8604 // Will not be null when 'Provided_Arhive_File' is string.Empty or Null, it will default to the Location Folder
#pragma warning disable CS8601 //.NET 6
#endif
                Folder_Path = File.Exists(Provided_Arhive_File) ? Path.GetDirectoryName(Provided_Arhive_File) : Path.Combine(Location_Folder, ".Launcher", "Downloads");
                File_Name = !string.IsNullOrWhiteSpace(Provided_File_Name) ? Provided_File_Name : Path.GetFileName(Web_Address);
                File_Path = File.Exists(Provided_Arhive_File) ? Provided_Arhive_File : Path.Combine(Folder_Path, File_Name);
                if (Provided_File_Size > 0)
                {
                    Web_File_Size = Provided_File_Size;
                }

                /* Game Folder */
                if (!Directory.Exists(Location_Folder))
                {
                    Directory.CreateDirectory(Location_Folder);
                }
                /* Game Pack Cache Folder */
                if (!Directory.Exists(Folder_Path))
                {
                    Directory.CreateDirectory(Folder_Path);
#if !NETFRAMEWORK
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8601 //.NET 6
#endif
                }
                /* Game Pack File */
                if (!File.Exists(File_Path))
                {
                    File.Create(File_Path).Close();
                }
                else
                {
                    File_Size = new FileInfo(File_Path).Length;

                    if (File_Size > Web_File_Size)
                    {
                        File.Delete(File_Path);
                        File_Size = 0;
                        File.Create(File_Path).Close();
                    }
                    else if (File_Size == Web_File_Size && File_Removal)
                    {
                        if(Hashes.Hash_SHA(File_Path) != File_Hash)
                        {
                            File.Delete(File_Path);
                            File_Size = 0;
                            File.Create(File_Path).Close();
                        }
                    }
                }

                if (File_Size == Web_File_Size)
                {
                    if ((this.Live_Progress != null) && !Cancel)
                    {
                        this.Live_Progress(this,
                            new Download_Data_Progress_EventArgs(Web_File_Size, File_Size, Web_File_Size_Remaining, Start_Time, Error_Rate));
                    }

                    string Caluated_Local_File_Hash = Hashes.Hash_SHA(File_Path);

                    if (Caluated_Local_File_Hash == File_Hash)
                    {
                        if ((this.Complete != null) && !Cancel)
                        {
                            this.Complete(this, new Download_Data_Complete_EventArgs(true, File_Path, DateTime.Now));
                        }
                        else
                        {
                            Cancel = true;
                        }
                    }
                    else
                    {
                        throw new Downloaded_File_Hash_Invalid_Exception("Local File does not match Provided Hash. " +
                            "Excepted: " + File_Hash + " File: " + 
                            (string.IsNullOrWhiteSpace(Caluated_Local_File_Hash) ? "Null String" : Caluated_Local_File_Hash));
                    }
                }
                else
                {
                    /* Set Time when we Started Request */
                    if (Start_Time == default)
                    {
                        Start_Time = DateTime.Now;
                    }

                    /* Create a new HttpWebRequest instance. */
#if !NETFRAMEWORK
#pragma warning disable SYSLIB0014 // Type or member is obsolete
#endif
                    Live_Request = (HttpWebRequest)WebRequest.Create(Web_Address);
#if !NETFRAMEWORK
#pragma warning restore SYSLIB0014 // Type or member is obsolete
#endif
                    Live_Request.UserAgent = Download_Settings.Header;
                    Live_Request.Headers["X-UserAgent"] = Download_Settings.Header;

                    if (File_Size > 0 && Provided_File_Size > 0)
                    {
                        Live_Request.AddRange(File_Size, Provided_File_Size);
                    }

                    Live_Request.Timeout = Download_Settings.Launcher_WebCall_Timeout();

                    /* Read the file in chunks of 'Download_Block_Size' */
                    byte[] Live_Buffer = new byte[Download_Block_Size];
                    int Bytes_Read;
                    File_Size_Live = File_Size;

                    /* Send the request and get the response. */
                    Live_Response = (HttpWebResponse)Live_Request.GetResponse();

                    if (Live_Response == null)
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - Received Response is Null",
                            Web_Address));
                    }
                    else if (Live_Response.ContentType.Contains("text/html"))
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - A web page was returned from the web server.",
                            Web_Address));
                    }
                    else if (Live_Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - File not Found on web server.",
                            Web_Address));
                    }
                    else
                    {
                        if (Live_Response.StatusCode != HttpStatusCode.PartialContent)
                        {
                            if (File_Size != 0)
                            {
                                File.Delete(File_Path);
                                File_Size = 0;
                                File.Create(File_Path).Close();
                            }
                        }

                        /* Get the stream containing the response. */
                        using (Stream Live_Stream = Live_Response.GetResponseStream())
                        {
                            /* Use a FileStream to write the file to the local file system. */
                            Live_Writer = new FileStream(File_Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            Web_File_Size_Remaining = Live_Response.ContentLength;

                            while ((Bytes_Read = Live_Stream.Read(Live_Buffer, 0, Download_Block_Size)) > 0)
                            {
                                if (Cancel)
                                {
                                    break;
                                }
                                else
                                {
                                    Live_Writer.Write(Live_Buffer, 0, Bytes_Read);
                                    File_Size_Live += Bytes_Read;

                                    if ((this.Live_Progress != null) && !Cancel)
                                    {
                                        this.Live_Progress(this,
                                            new Download_Data_Progress_EventArgs(Web_File_Size, File_Size_Live, Web_File_Size_Remaining, Start_Time, Error_Rate));
                                    }
                                    
                                    if (!Disable_Download_Status_Information && !Cancel)
                                    {
                                        Download_Status_Information = new Download_Information()
                                        {
                                            File_Size_Total = Web_File_Size,
                                            File_Size_Current = File_Size_Live,
                                            File_Size_Remaining = Web_File_Size_Remaining,
                                            Download_Percentage = (int)((((double)File_Size_Live) / Web_File_Size) * 100),
                                            Start_Time = Start_Time,
                                            Download_Attempts = Error_Rate
                                        };
                                    }

                                    if (Cancel)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (!Disable_Download_Status_Information && !Cancel)
                            {
                                Download_Status_Information = new Download_Information()
                                {
                                    File_Size_Total = Web_File_Size,
                                    File_Size_Current = File_Size_Live,
                                    File_Size_Remaining = Web_File_Size_Remaining,
                                    Download_Percentage = (int)((((double)File_Size_Live) / Web_File_Size) * 100),
                                    Start_Time = Start_Time,
                                    End_Time = DateTime.Now,
                                    Download_Complete = true,
                                    Download_Attempts = Error_Rate
                                };
                            }

                            if ((this.Complete != null) && !Cancel)
                            {
                                if (Live_Response != null)
                                {
                                    Live_Response.Close();
                                    Live_Response.Dispose();
                                    Live_Response = null;
                                }

                                if (Live_Writer != null)
                                {
                                    Live_Writer.Flush();
                                    Live_Writer.Close();
                                    Live_Writer.Dispose();
                                    Live_Writer = null;
                                }

                                string Caluated_Local_File_Hash = Hashes.Hash_SHA(File_Path);

                                if (Caluated_Local_File_Hash == File_Hash)
                                {
                                    if ((this.Complete != null) && !Cancel)
                                    {
                                        this.Complete(this, new Download_Data_Complete_EventArgs(true, File_Path, DateTime.Now));
                                    }
                                    else
                                    {
                                        Cancel = true;
                                    }
                                }
                                /* If Current File Size is Smaller than the File on the Server OR 
                                 * If Current File Size is Greator than on the Server AND
                                 * If the Amount of Retrys is less than or equal to 'Download_Retry_Attempts'
                                 * 
                                 * Go ahead and try the download
                                 */
                                else if (((File_Size_Live < Web_File_Size) || (File_Size_Live > Web_File_Size)) && (Error_Rate <= Download_Retry_Attempts))
                                {
                                    /* TODO: Check why this function causes issues for Unix Builds and for those who have a connection that can cause it to fail */
                                    Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_File_Name, Error_Rate + 1);
                                }
                                else if (Error_Rate <= Download_Retry_Attempts)
                                {
                                    throw new Downloaded_File_Hash_Invalid_Exception("Local File does not match Provided Hash. " +
                                        "Excepted: " + File_Hash + " File: " +
                                        (string.IsNullOrWhiteSpace(Caluated_Local_File_Hash) ? "Null String" : Caluated_Local_File_Hash));
                                }
                                else
                                {
                                    throw new Download_Client_Exception("Download Client was being Interrupted. Please Manually Retry.");
                                }
                            }
                            else if ((Live_Response != null) && Cancel)
                            {
                                Live_Response.Close();
                                Live_Response.Dispose();
                                Live_Response = null;
                            }
                        }
                    }
                }
            }
#if !DEBUG
            catch (WebException Error_Caught)
            {
                Exception_Router(true, Error_Caught, true);
            }
            catch (UriFormatException Error)
            {
                Exception_Router(true, new ArgumentException(
                    string.Format("Could not parse the URL \"{0}\" - it's either malformed or is an unknown protocol.", Web_Address), Error));
            }
            catch (Exception Error)
            {
                Exception_Router(true, Error);
            }
#endif
            finally
            {
                if (Live_Writer != null)
                {
                    Live_Writer.Flush();
                    Live_Writer.Close();
                    Live_Writer.Dispose();
                    Live_Writer = null;
                }
            }
        }
        /// <summary>
        /// Download a file from a list or URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        public void Download(List<string> Web_Address_List)
        {
            this.Download(Web_Address_List, string.Empty, -1);
        }
        /// <summary>
        /// Download a file from a list or URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        public void Download(List<string> Web_Address_List, string Location_Folder, long Provied_File_Size, string Provided_Archive_File = "")
        {
            // validate input
            if (Web_Address_List == null)
            {
                Exception_Router(true, new ArgumentNullException("Web_Address_List"));
            }
            else if (Web_Address_List.Count == 0)
            {
                Exception_Router(true, new ArgumentException("EMPTY Web_Address_List"));
            }
            else
            {
                // try each url in the list.
                // if one succeeds, we are done.
                // if any fail, move to the next.
                Exception? Web_Address_Exception = null;
                foreach (string Single_Web_Address in Web_Address_List)
                {
                    Web_Address_Exception = null;
                    try
                    {
                        Download(Single_Web_Address, Location_Folder, Provided_Archive_File, Provied_File_Size);
                    }
                    catch (Exception e)
                    {
                        Web_Address_Exception = e;
                    }
                    // If we got through that without an exception, we found a good url
                    if (Web_Address_Exception == null)
                    {
                        break;
                    }
                }

                if (Web_Address_Exception != null)
                {
                    Exception_Router(true, Web_Address_Exception);
                }
            }
        }
#if !NETFRAMEWORK
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#endif
        /// <summary>
        /// Asynchronously download a file from the url.
        /// </summary>
        public void AsyncDownload(string Web_Address)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(this.WaitCallbackMethod), new string[] { Web_Address, string.Empty });
        }
        /// <summary>
        /// Asynchronously download a file from the url to the destination folder.
        /// </summary>
        public void AsyncDownload(string Web_Address, string Location_Folder)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(this.WaitCallbackMethod), new string[] { Web_Address, Location_Folder });
        }
        /// <summary>
        /// Asynchronously download a file from a list or URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        public void AsyncDownload(List<string> Web_Address_List, string Location_Folder)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(this.WaitCallbackMethod), new object[] { Web_Address_List, Location_Folder });
        }
        /// <summary>
        /// Asynchronously download a file from a list or URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        public void AsyncDownload(List<string> Web_Address_List)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(this.WaitCallbackMethod), new object[] { Web_Address_List, string.Empty });
        }
#if !NETFRAMEWORK
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#endif
        /// <summary>
        /// A WaitCallback used by the AsyncDownload methods.
        /// </summary>
        private void WaitCallbackMethod(object Object_Data)
        {
            if (Object_Data != null)
            {
                // Can either be a string array of two strings (url and dest folder),
                // or an object array containing a list<string> and a dest folder
                if (Object_Data is string[])
                {
                    string[]? List_Strings = Object_Data as string[];
                    if (List_Strings != null)
                    {
                        if (List_Strings.Length > 0 && List_Strings.Length <= 4)
                        {
                            this.Download(List_Strings[0], List_Strings[1], List_Strings[2], long.TryParse(List_Strings[3], out long Provied_File_Size) ? Provied_File_Size : -1);
                        }
                    }
                }
                else
                {
                    object[]? List_Objects = Object_Data as object[];
                    if (List_Objects != null)
                    {
                        if (List_Objects.Length > 0 && List_Objects.Length <= 3)
                        {
                            List<string> Web_Address_List = (List_Objects[0] as List<string>) ?? new List<string>();
                            string? Location_Folder = List_Objects[1] as string;
                            if (!string.IsNullOrWhiteSpace(Location_Folder))
                            {
#pragma warning disable CS8604 // Possible null reference argument.
                                this.Download(Web_Address_List, Location_Folder, long.TryParse(List_Objects[2] as string, out long Provied_File_Size) ? Provied_File_Size : -1);
#pragma warning restore CS8604 // Possible null reference argument.
                            }
                        }
                    }
                }
            }
        }
    }
}