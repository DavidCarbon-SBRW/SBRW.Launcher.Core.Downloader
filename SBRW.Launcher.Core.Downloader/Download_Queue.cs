using SBRW.Launcher.Core.Downloader.EventArg_;
using SBRW.Launcher.Core.Downloader.Exception_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Queue
    {
        /// <summary>
        /// 
        /// </summary>
        private int Download_Block_Size { get { return 10240 * 5; } }
        /// <summary>
        /// 
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Download_Location 
        { 
            get 
            {
                if (Download_System != null)
                {
                    return Download_System.Full_Path; 
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private Download_Client? Download_System { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; internal set; }
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
        internal void eException_Router(bool Event_Hook, Exception Exception_Caught)
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
        /// <param name="Web_Address"></param>
        /// <param name="Location_File_Path"></param>
        /// <returns></returns>
        public void Download(string Web_Address, string Location_File_Path)
        {
            Download(Web_Address, Location_File_Path, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_File_Path"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <returns></returns>
        public void Download(string Web_Address, string Location_File_Path, string Provided_Arhive_File)
        {
            Download(Web_Address, Location_File_Path, Provided_Arhive_File, -1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <returns></returns>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Provided_Proxy_Url"></param>
        /// <returns></returns>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Provided_Proxy_Url"></param>
        /// <param name="Provided_File_Name"></param>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url, string Provided_File_Name)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, Provided_File_Name, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Provided_Proxy_Url"></param>
        /// <param name="Provided_File_Name"></param>
        /// <param name="Local_Cache_Policy"></param>
        /// <returns></returns>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url, string Provided_File_Name, RequestCachePolicy? Local_Cache_Policy)
        {
            Download(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, Provided_File_Name, Local_Cache_Policy, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Provided_Proxy_Url"></param>
        /// <param name="Provided_File_Name"></param>
        /// <param name="Local_Cache_Policy"></param>
        /// <param name="Local_Web_Proxy"></param>
        public void Download(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url, string Provided_File_Name, RequestCachePolicy? Local_Cache_Policy, IWebProxy? Local_Web_Proxy)
        {
            try
            {
                Start_Time = DateTime.Now;
                Download_System = Download_Client.Create(Web_Address, !string.IsNullOrWhiteSpace(Location_Folder) ? Location_Folder.Replace("file:///", string.Empty).Replace("file://", string.Empty) : string.Empty, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, Provided_File_Name, Local_Cache_Policy, Local_Web_Proxy);

                byte[] buffer = new byte[Download_Block_Size];
                int readCount;

                long totalDownloaded = Download_System.StartPoint;

                while (((readCount = Download_System.DownloadStream.Read(buffer, 0, Download_Block_Size)) > 0) && !Cancel)
                {
                    if (Cancel)
                    {
                        Download_System.Close();
                        break;
                    }

                    totalDownloaded += readCount;

                    SaveToFile(buffer, readCount, Download_System.Full_Path);

                    if (Download_System.IsProgressKnown && (this.Live_Progress != null)) 
                    {
                        this.Live_Progress(this, new Download_Data_Progress_EventArgs(Download_System.FileSize, totalDownloaded, Start_Time));
                    }

                    if (Cancel)
                    {
                        Download_System.Close();
                        break;
                    }
                }

                if (this.Complete != null && !Cancel)
                {
                    this.Complete(this, new Download_Data_Complete_EventArgs(true, Download_System.Full_Path, DateTime.Now));
                }
            }
            finally
            {
                if (Download_System != null)
                {
                    Download_System.Close();
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
                //Exception_Router(true, new ArgumentNullException("Web_Address_List"));
            }
            else if (Web_Address_List.Count == 0)
            {
                //Exception_Router(true, new ArgumentException("EMPTY Web_Address_List"));
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
                    //Exception_Router(true, Web_Address_Exception);
                }
            }
        }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <param name="fileName"></param>
        /// <exception cref="ArgumentException"></exception>
        private void SaveToFile(byte[] buffer, int count, string fileName)
        {
            FileStream? Live_File = null;

            try
            {
                Live_File = File.Open(fileName, FileMode.Append, FileAccess.Write);
                Live_File.Write(buffer, 0, count);
            }
            finally
            {
                if (Live_File != null)
                {
                    Live_File.Close();
                }
            }
        }
    }
}