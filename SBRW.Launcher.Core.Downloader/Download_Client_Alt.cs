using System;
using System.IO;
using System.Net.Cache;
using System.Net;
using SBRW.Launcher.Core.Downloader.EventArg_;
using SBRW.Launcher.Core.Downloader.Exception_;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Client_Alt
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RequestCachePolicy? Cache_Policy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string File_Name { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Folder_Path { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 
        /// </summary>
        public string Full_Path { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string File_Arhive_Path { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public long File_Arhive_Size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; set; } = DateTime.Now;
        /// <summary>
        /// 
        /// </summary>
        public string Url_Address { get; set; } = "http://localhost";
        /// <summary>
        /// 
        /// </summary>
        public string Url_Address_Proxy { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public IWebProxy? Web_Proxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public HttpWebRequest? Web_Client { get; set; }
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
            Download(Url_Address, Folder_Path, File_Arhive_Path, File_Arhive_Size, Url_Address_Proxy, File_Name, Cache_Policy, Web_Proxy);
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
                if (Web_Client == default)
                {
                    // Create a new HttpWebRequest instance.
                    Web_Client = (HttpWebRequest)WebRequest.Create(Web_Address);
                }

                File_Name = !string.IsNullOrWhiteSpace(Provided_File_Name) ? Provided_File_Name : Path.GetFileName(Web_Address);
                Folder_Path = File.Exists(Provided_Arhive_File) ? Path.GetDirectoryName(Provided_Arhive_File) : Path.Combine(Location_Folder, ".Launcher", "Downloads");
                Full_Path = File.Exists(Provided_Arhive_File) ? Provided_Arhive_File : Path.Combine(Folder_Path, File_Name);

                if (File.Exists(Full_Path))
                {
                    File_Arhive_Size = new FileInfo(Full_Path).Length;
                }

                // Set the start point for the download.
                Web_Client.AddRange(File_Arhive_Size);

                using (WebResponse Live_Response = Web_Client.GetResponse())
                {
                    using (Stream Live_Stream = Live_Response.GetResponseStream())
                    {
                        // Use a BinaryReader to read the file in small chunks.
                        using (BinaryReader Live_Reader = new BinaryReader(Live_Stream))
                        {
                            // Use a FileStream to write the file to the local file system.
                            using (FileStream Live_Writer = new FileStream(Full_Path, FileMode.Append))
                            {
                                // Read the file in chunks of 1KB.
                                byte[] Live_Buffer = new byte[1024];
                                int Live_Bytes_Read;
                                while ((Live_Bytes_Read = Live_Reader.Read(Live_Buffer, 0, Live_Buffer.Length)) > 0)
                                {
                                    // Write the chunk to the local file.
                                    Live_Writer.Write(Live_Buffer, 0, Live_Bytes_Read);

                                    if ((this.Live_Progress != null) && !Cancel)
                                    {
                                        this.Live_Progress(this, new Download_Data_Progress_EventArgs(Live_Response.ContentLength, File_Arhive_Size + Live_Writer.Length, Start_Time));
                                    }

                                    if (Cancel)
                                    {
                                        Live_Stream.Close();
                                        Live_Stream.Dispose();
                                        Live_Response.Close();
                                        Live_Response.Dispose();
                                        break;
                                    }

                                    /* Calculate the download progress.
                                    double progress = writer.Length / (double);

                                    // Update the progress bar value.
                                    progressBar.Value = (int)(progress * 100);

                                    // Show the percentage downloaded.
                                    label.Text = (int)(progress * 100) + "%";
                                    */
                                }

                                if (this.Complete != null && !Cancel)
                                {
                                    this.Complete(this, new Download_Data_Complete_EventArgs(true, Full_Path, DateTime.Now));
                                }
                            }
                        }
                    }
                }
            }
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
        }
    }
}
