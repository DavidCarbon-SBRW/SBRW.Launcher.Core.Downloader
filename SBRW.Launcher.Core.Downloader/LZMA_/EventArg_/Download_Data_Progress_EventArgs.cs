using System;

namespace SBRW.Launcher.Core.Downloader.LZMA_.EventArg_
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
        public string? Download_State { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Total_Uncompressed { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Current { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Current_Divide_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Current_Divide_Total_Uncompressed { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Current"></param>
        /// <param name="Received_Start_Time"></param>
        public Download_Data_Progress_EventArgs(long Received_File_Size_Current, long Received_File_Size_Total, long Received_File_Size_Total_Uncompressed long Calulated_Current_Divide_Total, long Calulated_Current_Divide_Total_Uncompressed, int Calulated_Percentage, DateTime Received_Start_Time)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Current;
            this.Download_Percentage = Calulated_Percentage;
            this.Start_Time = Received_Start_Time;
            this.File_Size_Current_Divide_Total = Calulated_Current_Divide_Total;
        }
    }
}
