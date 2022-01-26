using SBRW.Launcher.Core.Downloader.EventArg_;
using SBRW.Launcher.Core.Downloader.Exception_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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
        public string? Download_Location { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public IWebProxy? Web_Proxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private Download_Data? Download_System { get; set; }
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
        internal void Exception_Router(bool Event_Hook, Exception Exception_Caught)
        {
            if (this.Internal_Error != null && Event_Hook)
            {
                this.Internal_Error(this, new Download_Exception_EventArgs(Exception_Caught, DateTime.Now));
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
        /// <param name="File_Name"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Download(string Web_Address, string Location_Folder = "", string File_Name = "")
        {
            try
            {
                Start_Time = DateTime.Now;

                Download_System = Download_Data.Create(Web_Address, Location_Folder, this.Web_Proxy);

                Location_Folder = Location_Folder.Replace("file:///", string.Empty).Replace("file://", string.Empty);

                this.Download_Location = string.IsNullOrWhiteSpace(File_Name) ?
                    Path.Combine(Location_Folder, Path.GetFileName(Download_System.Web_Response.ResponseUri.ToString())) : Path.Combine(Location_Folder, File_Name);

                if (!File.Exists(Download_Location))
                {
                    File.Create(Download_Location).Close();
                }

                byte[] buffer = new byte[Download_Block_Size];
                int readCount;

                long totalDownloaded = Download_System.StartPoint;

                while ((readCount = Download_System.DownloadStream.Read(buffer, 0, Download_Block_Size)) > 0)
                {
                    if (Cancel)
                    {
                        Download_System.Close();
                        break;
                    }

                    totalDownloaded += readCount;

                    SaveToFile(buffer, readCount, this.Download_Location);

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

                if (this.Complete != null)
                {
                    this.Complete(this, new Download_Data_Complete_EventArgs(!Cancel, Download_Location, DateTime.Now));
                }
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
            this.Download(Web_Address_List, string.Empty);
        }
        /// <summary>
        /// Download a file from a list or URLs. If downloading from one of the URLs fails,
        /// another URL is tried.
        /// </summary>
        public void Download(List<string> Web_Address_List, string Location_Folder)
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
                        Download(Single_Web_Address, Location_Folder);
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
                    this.Download(List_Strings[0], List_Strings[1]);
                }
                else
                {
                    object[] List_Objects = Object_Data as object[];
                    List<string> Web_Address_List = (List_Objects[0] as List<string>) ?? new List<string>();
                    string Location_Folder = List_Objects[1] as string;
                    this.Download(Web_Address_List, Location_Folder);
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