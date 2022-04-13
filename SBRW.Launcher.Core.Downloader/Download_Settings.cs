namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Settings
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
    }
}
