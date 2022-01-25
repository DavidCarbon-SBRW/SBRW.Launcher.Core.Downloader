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
        private int DownloadBlockSize { get { return 10240 * 5; } }
        private bool Cancel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Download_Location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IWebProxy? Web_Proxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public event Download_Data_Delegates.Download_Data_Progress_Handler? ProgressChanged;
        /// <summary>
        /// /
        /// </summary>
        public event EventHandler? DownloadComplete;

        private void OnDownloadComplete()
        {
            if (this.DownloadComplete != null) { this.DownloadComplete(this, new EventArgs()); }
        }

        private static Download_Data? Download_System { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="File_Name"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Download(string Web_Address, string Location_Folder = "", string File_Name = "")
        {
            this.Cancel = false;

            try
            {
                Download_System = Download_Data.Create(Web_Address, Location_Folder, this.Web_Proxy);

                Location_Folder = Location_Folder.Replace("file:///", string.Empty).Replace("file://", string.Empty);

                this.Download_Location = string.IsNullOrWhiteSpace(File_Name) ?
                    Path.Combine(Location_Folder, Path.GetFileName(Download_System.Response.ResponseUri.ToString())) : Path.Combine(Location_Folder, File_Name);

                if (!File.Exists(Download_Location))
                {
                    FileStream fs = File.Create(Download_Location);
                    fs.Close();
                }

                byte[] buffer = new byte[DownloadBlockSize];
                int readCount;

                long totalDownloaded = Download_System.StartPoint;
                bool gotCanceled = false;

                while ((int)(readCount = Download_System.DownloadStream.Read(buffer, 0, DownloadBlockSize)) > 0)
                {
                    if (Cancel)
                    {
                        gotCanceled = true;
                        Download_System.Close();
                        break;
                    }

                    totalDownloaded += readCount;

                    SaveToFile(buffer, readCount, this.Download_Location);

                    if (Download_System.IsProgressKnown) { RaiseProgressChanged(totalDownloaded, Download_System.FileSize); }

                    if (Cancel)
                    {
                        gotCanceled = true;
                        Download_System.Close();
                        break;
                    }
                }

                if (!gotCanceled)
                {
                    OnDownloadComplete();
                }
            }
            catch (UriFormatException e)
            {
                throw new ArgumentException(
                    string.Format("Could not parse the URL \"{0}\" - it's either malformed or is an unknown protocol.", Web_Address), e);
            }
            finally
            {
                if (Download_System != null)
                    Download_System.Close();
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
                throw new ArgumentNullException("Web_Address_List");
            }
            else if (Web_Address_List.Count == 0)
            {
                throw new ArgumentException("EMPTY Web_Address_List");
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
                    throw Web_Address_Exception;
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

        private void SaveToFile(byte[] buffer, int count, string fileName)
        {
            FileStream? f = null;

            try
            {
                f = File.Open(fileName, FileMode.Append, FileAccess.Write);
                f.Write(buffer, 0, count);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    string.Format("Error trying to save file \"{0}\": {1}", fileName, e.Message), e);
            }
            finally
            {
                if (f != null)
                    f.Close();
            }
        }

        private void RaiseProgressChanged(long current, long target)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, new Download_Data_EventArgs(target, current));
            } 
        }
    }
}