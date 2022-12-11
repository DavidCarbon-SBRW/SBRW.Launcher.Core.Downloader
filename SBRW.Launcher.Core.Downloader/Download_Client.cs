using System;
using System.IO;
using System.Net;
using System.Net.Cache;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// Constains the connection to the file server and other statistics about a file
    /// that's downloading.
    /// </summary>
    public class Download_Client
    {
        /// <summary>
        /// 
        /// </summary>
        private WebResponse? Web_Response { get; set; }
        private Stream? Live_Stream { get; set; }
        private long Data_Size { get; set; }
        private long Data_Start { get; set; }
        private IWebProxy? Web_Proxy { get; set; }
        private RequestCachePolicy? Cache_Policy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static string File_Name { get; internal set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public static string Folder_Name { get; internal set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public static string Full_Path { get; internal set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <returns></returns>
        public static Download_Client Create(string Web_Address)
        {
            return Create(Web_Address, AppDomain.CurrentDomain.BaseDirectory);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <returns></returns>
        public static Download_Client Create(string Web_Address, string Location_Folder)
        {
            return Create(Web_Address, Location_Folder, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <returns></returns>
        public static Download_Client Create(string Web_Address, string Location_Folder, string Provided_Arhive_File)
        {
            return Create( Web_Address, Location_Folder, Provided_Arhive_File, - 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_Arhive_File"></param>
        /// <param name="Provided_File_Size"></param>
        /// <returns></returns>
        public static Download_Client Create(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size)
        {
            return Create(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, string.Empty);
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
        public static Download_Client Create(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url)
        {
            return Create(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, string.Empty);
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
        /// <returns></returns>
        public static Download_Client Create(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url, string Provided_File_Name)
        {
            return Create(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, Provided_File_Name, null);
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
        public static Download_Client Create(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url, string Provided_File_Name, RequestCachePolicy? Local_Cache_Policy)
        {
            return Create(Web_Address, Location_Folder, Provided_Arhive_File, Provided_File_Size, Provided_Proxy_Url, Provided_File_Name, Local_Cache_Policy, null);
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
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Download_Client Create(string Web_Address, string Location_Folder, string Provided_Arhive_File, long Provided_File_Size, string Provided_Proxy_Url, string Provided_File_Name, RequestCachePolicy? Local_Cache_Policy, IWebProxy? Local_Web_Proxy)
        {
            // This is what we will return
            Download_Client Data_Recevied = new Download_Client();
            if (Local_Web_Proxy != null)
            {
                Data_Recevied.Web_Proxy = Local_Web_Proxy;
            }
            if (Local_Cache_Policy != null)
            {
                Data_Recevied.Cache_Policy = Local_Cache_Policy;
            }

            long Size_Recevied = Data_Recevied.GetFileSize(Web_Address, Provided_File_Size);
            Data_Recevied.Data_Size = Size_Recevied;

            WebRequest Data_Request = Data_Recevied.GetRequest(Web_Address, Provided_Proxy_Url);
            bool Web_Response_Error = false;
            try
            {
                Data_Recevied.Web_Response = Data_Request.GetResponse();
            }
            catch (WebException)
            {
                Web_Response_Error = true;
                throw;
            }
            catch (Exception)
            {
                Web_Response_Error = true;
                throw;
            }

            if(!Web_Response_Error)
            {
                if (Data_Recevied.Web_Response is HttpWebResponse)
                {
                    HttpWebResponse? httpResponse = Data_Recevied.Web_Response as HttpWebResponse;

                    // If Received Response Is Null, throw an Error
                    if (httpResponse == null)
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - Received Response is Null",
                            Web_Address));
                    }

                    // If it's an HTML page, it's probably an error page. Comment this
                    // out to enable downloading of HTML pages.
                    if (httpResponse.ContentType.Contains("text/html") || httpResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - a web page was returned from the web server.",
                            Web_Address));
                    }
                }
                else if (Data_Recevied.Web_Response is FtpWebResponse)
                {
                    FtpWebResponse? ftpResponse = Data_Recevied.Web_Response as FtpWebResponse;

                    if (ftpResponse == null)
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - FTP server Responsed with Null",
                            Web_Address));
                    }

                    if (ftpResponse.StatusCode == FtpStatusCode.ConnectionClosed)
                    {
                        throw new Exception(
                            string.Format("Could not download \"{0}\" - FTP server closed the connection.", Web_Address));
                    }
                }

                // Take the name of the file given to use from the web server.
                File_Name = !string.IsNullOrWhiteSpace(Provided_File_Name) ? Provided_File_Name : Path.GetFileName(new Uri(Web_Address).Equals(Data_Recevied.Web_Response.ResponseUri) ? Web_Address : Data_Recevied.Web_Response.ResponseUri.OriginalString);
                Folder_Name = File.Exists(Provided_Arhive_File) ? Path.GetDirectoryName(Provided_Arhive_File) : Path.Combine(Location_Folder, ".Launcher", "Downloads");
                Full_Path = File.Exists(Provided_Arhive_File) ? Provided_Arhive_File : Path.Combine(Folder_Name, File_Name);

                // If we don't know how big the file is supposed to be,
                // we can't resume, so delete what we already have if something is on disk already.
                if (!Data_Recevied.IsProgressKnown && File.Exists(Full_Path))
                {
                    File.Delete(Full_Path);
                }

                if (Data_Recevied.IsProgressKnown && File.Exists(Full_Path))
                {
                    // We only support resuming on http Data_Requestuests
                    if (!(Data_Recevied.Web_Response is HttpWebResponse))
                    {
                        File.Delete(Full_Path);
                        Data_Recevied.Data_Start = 0;
                    }
                    else
                    {
                        // Try and start where the file on disk left off
                        Data_Recevied.Data_Start = new FileInfo(Full_Path).Length;

                        // If we have a file that's bigger than what is online, then something 
                        // strange happened. Delete it and start again.
                        if (Data_Recevied.Data_Start > Size_Recevied)
                        {
                            File.Delete(Full_Path);
                            // Reset Data_Start File Size to Correctly update the Download Percentage
                            Data_Recevied.Data_Start = 0;
                        }
                        else if (Data_Recevied.Data_Start < Size_Recevied)
                        {
                            // Try and resume by creating a new Data_Requestuest with a new start position
                            if (Data_Recevied.Web_Response != null)
                            {
                                Data_Recevied.Web_Response.Close();
                                Data_Recevied.Web_Response.Dispose();
                            }

                            Data_Request = Data_Recevied.GetRequest(Web_Address, Provided_Proxy_Url);
                            ((HttpWebRequest)Data_Request).AddRange(Data_Recevied.Data_Start);
                            ((HttpWebRequest)Data_Request).Headers["X-MTNTR-HEADER-VAL"] = Data_Recevied.Data_Start.ToString();
                            ((HttpWebRequest)Data_Request).Headers["X-UserAgent"] = Download_Settings.Header;
                            if (Local_Cache_Policy != null)
                            {
                                ((HttpWebRequest)Data_Request).CachePolicy = Local_Cache_Policy;
                            }
                            Data_Recevied.Web_Response = Data_Request.GetResponse();

                            if (((HttpWebResponse)Data_Recevied.Web_Response).StatusCode != HttpStatusCode.PartialContent)
                            {
                                // They didn't support our resume request
                                File.Delete(Full_Path);
                                Data_Recevied.Data_Start = 0;
                            }
                        }
                    }
                }
            }

            return Data_Recevied;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Provided_File_Size"></param>
        /// <returns></returns>
        private long GetFileSize(string Web_Address, long Provided_File_Size)
        {
            WebResponse? Size_Response = null;
            long Received_Size = -1;

            try
            {
                Size_Response = GetRequest(Web_Address).GetResponse();
                Received_Size = Size_Response.ContentLength;

                if (Received_Size <= -1 && Provided_File_Size > -1)
                {
                    Received_Size = Provided_File_Size;
                }
            }
            catch (Exception)
            {
                if (Provided_File_Size > -1)
                {
                    Received_Size = Provided_File_Size;
                }
            }
            finally
            {
                if (Size_Response != null)
                {
                    Size_Response.Close();
                    Size_Response.Dispose();
                }
            }

            return Received_Size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Proxy_Url"></param>
        /// <returns></returns>
        private WebRequest GetRequest(string Web_Address, string Proxy_Url = "")
        {
            WebRequest Proxy_Request = WebRequest.Create(Web_Address);
            if (Proxy_Request is HttpWebRequest)
            {
                Proxy_Request.Headers["X-UserAgent"] = Download_Settings.Header;
                Proxy_Request.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    if (Uri.TryCreate(!string.IsNullOrWhiteSpace(Proxy_Url) ? Proxy_Url : Web_Address, UriKind.Absolute, out Uri? Uri_Result) && (Uri_Result.Scheme == Uri.UriSchemeHttp || Uri_Result.Scheme == Uri.UriSchemeHttps))
                    {
                        Proxy_Request.Proxy.GetProxy(Uri_Result);
                    }
                    else
                    {
                        Proxy_Request.Proxy.GetProxy(new Uri("http://www.google.com"));
                    }
                }
                catch
                {
                    Proxy_Request.Proxy.GetProxy(new Uri("http://www.google.com"));
                }
            }

            if (this.Cache_Policy != null)
            {
                Proxy_Request.CachePolicy = this.Cache_Policy;
            }

            if (this.Web_Proxy != null)
            {
                Proxy_Request.Proxy = this.Web_Proxy;
            }

            return Proxy_Request;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (this.Web_Response != null)
            {
                this.Web_Response.Close();
                this.Web_Response.Dispose();
            }
        }

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Stream DownloadStream
        {
            get
            {
                if ((this.Data_Start == this.Data_Size) || (this.Live_Stream == null && this.Web_Response == null))
                {
                    return Stream.Null;
                }
                else if (this.Live_Stream == null)
                {
#pragma warning disable CS8602
                    this.Live_Stream = Web_Response.GetResponseStream();
#pragma warning restore CS8602
                }

                return this.Live_Stream;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public long FileSize
        {
            get
            {
                return this.Data_Size;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public long StartPoint
        {
            get
            {
                return this.Data_Start;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsProgressKnown
        {
            get
            {
                // If the size of the remote url is -1, that means we
                // couldn't determine it, and so we don't know
                // progress information.
                return this.Data_Size > -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string FileName
        {
            get
            {
                return File_Name;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string FolderName
        {
            get
            {
                return Folder_Name;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string FullPath
        {
            get
            {
                return Full_Path;
            }
        }
        #endregion
    }
}
