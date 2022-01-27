using System;
using System.IO;
using System.Net;

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
        public WebResponse? Web_Response { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Stream? Live_Stream { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Data_Size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Data_Start { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IWebProxy? Web_Proxy { get; set; }
        /// <summary>
        /// Used by the factory method
        /// </summary>
        public Download_Client() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_response"></param>
        /// <param name="Web_Address"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ValidateResponse(WebResponse Received_response, string Web_Address)
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
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Provided_File_Size"></param>
        /// <returns></returns>
        public long GetFileSize(string Web_Address, long Provided_File_Size)
        {
            WebResponse? Size_Response = null;
            long Received_Size = -1;

            try
            {
                if (Provided_File_Size > -1)
                {
                    Received_Size = Provided_File_Size;
                }
                else
                {
                    Size_Response = GetRequest(Web_Address).GetResponse();
                    Received_Size = Size_Response.ContentLength;
                }
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
        public WebRequest GetRequest(string Web_Address)
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
