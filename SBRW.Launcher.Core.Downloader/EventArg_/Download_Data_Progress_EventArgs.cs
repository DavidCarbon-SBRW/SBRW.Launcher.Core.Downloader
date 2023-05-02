using System;

namespace SBRW.Launcher.Core.Downloader.EventArg_
{
    /// <summary>
    /// Progress of a downloading file.
    /// </summary>
    public class Download_Data_Progress_EventArgs : EventArgs
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
        public int Download_Attempts { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Current"></param>
        /// <param name="Received_File_Size_Remaining"></param>
        /// <param name="Received_Start_Time"></param>
        /// <param name="Attempts_for_Download"></param>
        public Download_Data_Progress_EventArgs(long Received_File_Size_Total, long Received_File_Size_Current,
            long Received_File_Size_Remaining, DateTime Received_Start_Time, int Attempts_for_Download)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Current;
            this.File_Size_Remaining = Received_File_Size_Remaining;
            this.Download_Percentage = (int)((((double)Received_File_Size_Current) / Received_File_Size_Total) * 100);
            this.Start_Time = Received_Start_Time;
            this.Download_Attempts = Attempts_for_Download;
        }
    }
}