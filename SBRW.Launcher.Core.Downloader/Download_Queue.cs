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
        public string Download_Location { get; internal set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public RequestCachePolicy? Cache_Policy { get; set; }
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
        /// <param name="Related_To_WebClient"></param>
        internal void Exception_Router(bool Event_Hook, Exception Exception_Caught, bool Related_To_WebClient = false)
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
        /// <param name="Location_Folder"></param>
        /// <param name="Provied_File_Size"></param>
        /// <param name="Local_Cache_Policy"></param>
        /// <param name="Provided_Proxy_Url"></param>
        /// <param name="Local_Web_Proxy"></param>
        public void Download(string Web_Address, string Location_Folder, long Provied_File_Size = -1, RequestCachePolicy? Local_Cache_Policy = null, string Provided_Proxy_Url = "", IWebProxy? Local_Web_Proxy = null)
        {
            try
            {
                Start_Time = DateTime.Now;

                Download_System = Download_Client.Create(Provied_File_Size, Web_Address, Location_Folder, Local_Cache_Policy, Provided_Proxy_Url, Local_Web_Proxy);

                Location_Folder = Location_Folder.Replace("file:///", string.Empty).Replace("file://", string.Empty);

                this.Download_Location = Download_System.Full_Path;

                if (!File.Exists(Download_System.Full_Path))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(Download_System.Full_Path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(Download_System.Full_Path));
                    }

                    File.Create(Download_System.Full_Path).Close();
                }

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
                    else
                    {
                        if (!Download_Settings.System_Unix)
                        {
                            GC.Collect();
                        }
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
                    else
                    {
                        if (!Download_Settings.System_Unix)
                        {
                            GC.Collect();
                        }
                    }
                }

                if (this.Complete != null && !Cancel)
                {
                    this.Complete(this, new Download_Data_Complete_EventArgs(true, Download_System.Full_Path, DateTime.Now));
                }
            }
            catch(WebException Error_Caught)
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
        public void Download(List<string> Web_Address_List, string Location_Folder, long Provied_File_Size)
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
                        Download(Single_Web_Address, Location_Folder, Provied_File_Size);
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
                    string[] List_Strings = Object_Data as string[];
                    this.Download(List_Strings[0], List_Strings[1], long.TryParse(List_Strings[2], out long Provied_File_Size) ? Provied_File_Size : -1);
                }
                else
                {
                    object[] List_Objects = Object_Data as object[];
                    List<string> Web_Address_List = (List_Objects[0] as List<string>) ?? new List<string>();
                    string Location_Folder = List_Objects[1] as string;
                    this.Download(Web_Address_List, Location_Folder, long.TryParse(List_Objects[2] as string, out long Provied_File_Size) ? Provied_File_Size : -1);
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
            catch (ArgumentException Error)
            {
                Exception_Router(true, Error);
            }
            catch (Exception Error)
            {
                Exception_Router(true, Error);
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