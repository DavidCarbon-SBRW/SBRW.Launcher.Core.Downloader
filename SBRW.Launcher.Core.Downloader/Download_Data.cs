using System;
using System.IO;
using System.Net;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// Constains the connection to the file server and other statistics about a file
    /// that's downloading.
    /// </summary>
    public class Download_Data
    {
        /// <summary>
        /// 
        /// </summary>
        public WebResponse? Web_Response { get; set; }
        private Stream? Live_Stream { get; set; }
        private long Data_Size { get; set; }
        private long Data_Start { get; set; }
        private IWebProxy? Web_Proxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="destFolder"></param>
        /// <returns></returns>
        public static Download_Data Create(string Web_Address, string destFolder)
        {
            return Create(Web_Address, destFolder);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Local_Web_Proxy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Download_Data Create(string Web_Address, string Location_Folder, IWebProxy? Local_Web_Proxy = null)
        {
            // This is what we will return
            Download_Data Data_Recevied = new Download_Data();
            if (Local_Web_Proxy != null)
            {
                Data_Recevied.Web_Proxy = Local_Web_Proxy;
            }

            long Size_Recevied = Data_Recevied.GetFileSize(Web_Address);
            Data_Recevied.Data_Size = Size_Recevied;

            WebRequest Data_Request = Data_Recevied.GetRequest(Web_Address);
            try
            {
                Data_Recevied.Web_Response = Data_Request.GetResponse();
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format(
                    "Error downloading \"{0}\": {1}", Web_Address, e.Message), e);
            }

            // Check to make sure the response isn't an error. If it is this method
            // will throw exceptions.
            ValidateResponse(Data_Recevied.Web_Response, Web_Address);

            // Take the name of the file given to use from the web server.
            string Location_Download = Path.Combine(Location_Folder, Path.GetFileName(Data_Recevied.Web_Response.ResponseUri.ToString()));

            // If we don't know how big the file is supposed to be,
            // we can't resume, so delete what we already have if something is on disk already.
            if (!Data_Recevied.IsProgressKnown && File.Exists(Location_Download))
            {
                File.Delete(Location_Download);
            }

            if (Data_Recevied.IsProgressKnown && File.Exists(Location_Download))
            {
                // We only support resuming on http Data_Requestuests
                if (!(Data_Recevied.Web_Response is HttpWebResponse))
                {
                    File.Delete(Location_Download);
                }
                else
                {
                    // Try and start where the file on disk left off
                    Data_Recevied.Data_Start = new FileInfo(Location_Download).Length;

                    // If we have a file that's bigger than what is online, then something 
                    // strange happened. Delete it and start again.
                    if (Data_Recevied.Data_Start > Size_Recevied)
                    {
                        File.Delete(Location_Download);
                    }
                    else if (Data_Recevied.Data_Start < Size_Recevied)
                    {
                        // Try and resume by creating a new Data_Requestuest with a new start position
                        Data_Recevied.Web_Response.Close();
                        Data_Request = Data_Recevied.GetRequest(Web_Address);
                        ((HttpWebRequest)Data_Request).AddRange((int)Data_Recevied.Data_Start);
                        ((HttpWebRequest)Data_Request).Headers["X-MTNTR-HEADER-VAL"] = (Data_Recevied.Data_Start).ToString();
                        ((HttpWebRequest)Data_Request).Headers["X-UserAgent"] = Download_Data_Support.Header;
                        Data_Recevied.Web_Response = Data_Request.GetResponse();

                        if (((HttpWebResponse)Data_Recevied.Web_Response).StatusCode != HttpStatusCode.PartialContent)
                        {
                            // They didn't support our resume request
                            File.Delete(Location_Download);
                            Data_Recevied.Data_Start = 0;
                        }
                    }
                }
            }
            return Data_Recevied;
        }
        /// <summary>
        /// Used by the factory method
        /// </summary>
        private Download_Data() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_response"></param>
        /// <param name="Received_Size"></param>
        /// <param name="Received_Start"></param>
        private Download_Data(WebResponse Received_response, long Received_Size, long Received_Start)
        {
            this.Web_Response = Received_response;
            this.Data_Size = Received_Size;
            this.Data_Start = Received_Start;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_response"></param>
        /// <param name="Received_Size"></param>
        /// <param name="Received_Start"></param>
        /// <param name="Received_Stream"></param>
        private Download_Data(WebResponse Received_response, long Received_Size, long Received_Start, Stream Received_Stream)
        {
            this.Web_Response = Received_response;
            this.Data_Size = Received_Size;
            this.Data_Start = Received_Start;
            this.Live_Stream = Received_Stream;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_response"></param>
        /// <param name="Web_Address"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateResponse(WebResponse Received_response, string Web_Address)
        {
            if (Received_response is HttpWebResponse)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)Received_response;
                // If it's an HTML page, it's probably an error page. Comment this
                // out to enable downloading of HTML pages.
                if (httpResponse.ContentType.Contains("text/html") || httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ArgumentException(
                        string.Format("Could not download \"{0}\" - a web page was returned from the web server.",
                        Web_Address));
                }
            }
            else if (Received_response is FtpWebResponse)
            {
                FtpWebResponse ftpResponse = (FtpWebResponse)Received_response;
                if (ftpResponse.StatusCode == FtpStatusCode.ConnectionClosed)
                {
                    throw new ArgumentException(
                        string.Format("Could not download \"{0}\" - FTP server closed the connection.", Web_Address));
                } 
            }
            // FileWebResponse doesn't have a status code to check.
        }
        /// <summary>
        /// Checks the file size of a remote file. If size is -1, then the file size
        /// could not be determined.
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <returns></returns>
        private long GetFileSize(string Web_Address)
        {
            WebResponse? Size_Response = null;
            long Received_Size = -1;

            try
            {
                Size_Response = GetRequest(Web_Address).GetResponse();
                Received_Size = Size_Response.ContentLength;
            }
            finally
            {
                if (Size_Response != null)
                {
                    Size_Response.Close();
                }
            }

            return Received_Size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <returns></returns>
        private WebRequest GetRequest(string Web_Address)
        {
            WebRequest Proxy_Request = WebRequest.Create(Web_Address);
            if (Proxy_Request is HttpWebRequest)
            {
                Proxy_Request.Headers["X-UserAgent"] = Download_Data_Support.Header;
                Proxy_Request.Credentials = CredentialCache.DefaultCredentials;
                Proxy_Request.Proxy.GetProxy(new Uri("http://www.google.com"));
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
        #endregion
    }
}
