using System;
using System.Collections.Generic;
using System.Text;

namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// 
    /// </summary>
    public class Extract_Information
    {
        /// <summary>
        /// 
        /// </summary>
        public int Extract_Percentage { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public int File_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public int File_Current { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string? File_Current_Name { get; internal set; }
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
        public bool Extract_Complete { get; internal set; }
    }
}
