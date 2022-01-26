using System;

namespace SBRW.Launcher.Core.Downloader.EventArg_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Extract_Complete_EventArgs : EventArgs
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
        /// <param name="Completion_Time"></param>
        public Download_Extract_Complete_EventArgs(bool Completion_State, DateTime Completion_Time)
        {
            this.Complete = Completion_State;
            this.Stop_Time = Completion_Time;
        }
    }
}
