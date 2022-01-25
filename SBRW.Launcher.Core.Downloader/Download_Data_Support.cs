using System;
using System.Collections.Generic;
using System.Text;

namespace SBRW.Launcher.Core.Downloader
{
    public class Download_Data_Support
    {
        private static string Version { get { return "0.0.0.1"; } }
        public static bool System_Unix { get; set; }
        public static bool Alternative_WebCalls { get; set;}
        internal static string Header { get { return "SBRW.Launcher.Core.Downloader.LZMA Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)"; } }
        internal static string Header_LZMA { get { return "SBRW.Launcher.Core.Downloader Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)"; } }
    }
}
