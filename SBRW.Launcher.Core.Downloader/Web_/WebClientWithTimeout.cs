using System;
using System.Net;

namespace SBRW.Launcher.Core.Downloader.Web_
{
    /// <summary>
    /// WebClient Extension
    /// </summary>
    public class WebClientWithTimeout : WebClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri Web_Address)
        {
            if (Download_Settings.System_Unix)
            {
                Web_Address = new UriBuilder(Web_Address)
                {
                    Scheme = Uri.UriSchemeHttp,
                    Port = Web_Address.IsDefaultPort ? -1 : Web_Address.Port /* -1 => default port for scheme */
                }.Uri;
            }

            ServicePointManager.FindServicePoint(Web_Address).ConnectionLeaseTimeout = (int)(Download_Settings.Launcher_WebCall_Timeout_Enable ?
                TimeSpan.FromSeconds(Download_Settings.Launcher_WebCall_Timeout_Cache + 1).TotalMilliseconds : TimeSpan.FromMinutes(1).TotalMilliseconds);
            HttpWebRequest Live_Request = (HttpWebRequest)WebRequest.Create(Web_Address);
            Live_Request.Headers["X-UserAgent"] = Download_Settings.Header_LZMA;
            Live_Request.UserAgent = Download_Settings.Header_LZMA;
            Live_Request.Timeout = (int)(Download_Settings.Launcher_WebCall_Timeout_Enable ?
                TimeSpan.FromSeconds(Download_Settings.Launcher_WebCall_Timeout_Cache).TotalMilliseconds : TimeSpan.FromSeconds(30).TotalMilliseconds);
            Live_Request.KeepAlive = false;

            return Live_Request;
        }
    }
}
