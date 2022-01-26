using System;

namespace SBRW.Launcher.Core.Downloader.Exception_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Data_Exception_EventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime Recorded_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Exception? Recorded_Exception { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Exception_Caught"></param>
        public Download_Data_Exception_EventArgs(Exception Exception_Caught)
        {
            this.Recorded_Exception = Exception_Caught;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Time_Caught"></param>
        public Download_Data_Exception_EventArgs(DateTime Time_Caught)
        {
            this.Recorded_Time = Time_Caught;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Exception_Caught"></param>
        /// <param name="Time_Caught"></param>
        public Download_Data_Exception_EventArgs(Exception Exception_Caught, DateTime Time_Caught)
        {
            this.Recorded_Exception = Exception_Caught;
            this.Recorded_Time = Time_Caught;
        }
    }
}
