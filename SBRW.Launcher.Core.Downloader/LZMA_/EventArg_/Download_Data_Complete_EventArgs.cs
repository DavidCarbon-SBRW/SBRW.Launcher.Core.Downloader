using System;

namespace SBRW.Launcher.Core.Downloader.LZMA_.EventArg_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Data_Complete_EventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Complete { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Stop_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Download_Location { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Completion_State"></param>
        /// <param name="Saved_Location"></param>
        /// <param name="Completion_Time"></param>
        public Download_Data_Complete_EventArgs(bool Completion_State, DateTime Completion_Time, string Saved_Location = "")
        {
            this.Complete = Completion_State;
            this.Download_Location = Saved_Location;
            this.Stop_Time = Completion_Time;
        }
    }
}
