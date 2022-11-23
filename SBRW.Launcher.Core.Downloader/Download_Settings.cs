using System;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public static class Download_Settings
    {
        private const string Version = "0.0.0.12";
        /// <summary>
        /// 
        /// </summary>
        public static bool System_Unix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static bool Alternative_WebCalls { get; set; }
        /// <summary>
        /// 
        /// </summary>
        internal const string Header = "SBRW.Launcher.Core.Downloader.LZMA Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)";
        /// <summary>
        /// 
        /// </summary>
        internal const string Header_LZMA = "SBRW.Launcher.Core.Downloader Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)";
        /// <summary>
        /// Global Boolen for Web Call Timeout before termining the connection
        /// </summary>
        public static bool Launcher_WebCall_Timeout_Enable { get; set; }
        /// <summary>
        /// Cached Internal Value
        /// </summary>
        internal static int Launcher_WebCall_Timeout_Cache { get; set; } = 30;
        /// <summary>
        /// Global Web Call Timeout before termining the connection
        /// </summary>
        /// <returns></returns>
        public static int Launcher_WebCall_Timeout() => Launcher_WebCall_Timeout_Cache;
        /// <summary>
        /// Global Web Call Timeout before termining the connection
        /// </summary>
        /// <param name="Provided_Seconds">Seconds in int</param>
        /// <returns></returns>
        public static int Launcher_WebCall_Timeout(int Provided_Seconds)
        {
            try
            {
                if (Provided_Seconds <= 0 || Provided_Seconds >= 180)
                {
                    return Launcher_WebCall_Timeout_Cache = 30;
                }
                else
                {
                    return Launcher_WebCall_Timeout_Cache = Provided_Seconds;
                }
            }
            catch (Exception)
            {
                return Launcher_WebCall_Timeout_Cache = 30;
            }
        }
    }
}
