using System;

namespace SBRW.Launcher.Core.Downloader.LZMA_.EventArg_
{
    /// <summary>
    /// Progress of a downloading file.
    /// </summary>
    public class Download_Extract_Progress_EventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public int Extract_Percentage { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Current { get; internal set; }
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
        /// <param name="Received_Extract_Percentage">Percentage of the Extraction Process</param>
        /// <param name="Received_Extract_File_Name">Current File Name</param>
        /// <param name="Received_File_Total">Total amount of Files</param>
        /// <param name="Received_File_Current">Current Count out of the Toal amount of Files</param>
        /// <param name="Received_Time">Extraction Start Time</param>
        public Download_Extract_Progress_EventArgs(int Received_Extract_Percentage, string Received_Extract_File_Name, long Received_File_Total, long Received_File_Current, DateTime Received_Time)
        {
            this.Extract_Percentage = Received_Extract_Percentage;
            this.File_Current_Name = Received_Extract_File_Name;
            this.File_Total = Received_File_Total;
            this.File_Current = Received_File_Current;
            this.Start_Time = Received_Time;
        }
    }
}
