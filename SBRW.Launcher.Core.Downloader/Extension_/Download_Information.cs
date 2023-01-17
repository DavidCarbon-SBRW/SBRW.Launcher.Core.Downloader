using System;
using System.Collections.Generic;
using System.Text;

namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Information
    {
        /// <summary>
        /// 
        /// </summary>
        public int Download_Percentage { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Current { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Remaining { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime End_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Download_Complete { get; internal set; }
    }
}
