using System;

namespace SBRW.Launcher.Core.Downloader.EventArg_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Raw_Removal_EventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Missing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Invalid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
