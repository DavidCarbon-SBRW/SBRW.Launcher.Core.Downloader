namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Settings
    {
        private static string Version { get { return "0.0.0.12"; } }
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
        internal static string Header { get { return "SBRW.Launcher.Core.Downloader.LZMA Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)"; } }
        /// <summary>
        /// 
        /// </summary>
        internal static string Header_LZMA { get { return "SBRW.Launcher.Core.Downloader Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)"; } }
    }
}
