using System;

namespace SBRW.Launcher.Core.Downloader
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
        /// <param name="Completion_State"></param>
        public Download_Data_Complete_EventArgs(bool Completion_State)
        {
            this.Complete = Completion_State;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Completion_State"></param>
        /// <param name="Completion_Time"></param>
        public Download_Data_Complete_EventArgs(bool Completion_State, DateTime Completion_Time)
        {
            this.Complete = Completion_State;
            this.Stop_Time = Completion_Time;
        }
    }
}
