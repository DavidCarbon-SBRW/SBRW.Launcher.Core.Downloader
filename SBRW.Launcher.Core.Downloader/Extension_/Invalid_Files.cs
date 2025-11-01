using System;
using System.Collections.Generic;
using System.Text;

namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// 
    /// </summary>
    public class Invalid_Files
    {
        /// <summary>
        /// 
        /// </summary>
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Path_Full { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Path_Truncated { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Download_Url { get; set; } = string.Empty;
    }
}
