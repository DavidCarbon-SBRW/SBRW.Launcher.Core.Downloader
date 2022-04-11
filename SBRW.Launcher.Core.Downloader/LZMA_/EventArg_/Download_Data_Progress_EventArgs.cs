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
        public string? File_Name { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long Bytes_To_Receive_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long Bytes_Received { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Bytes_Received_Over_Total { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_Bytes_Current"></param>
        /// <param name="Received_Bytes_Total"></param>
        /// <param name="Calulated_Current_Divide_Total"></param>
        /// <param name="Calulated_Percentage"></param>
        /// <param name="Received_File_Name"></param>
        /// <param name="Received_Start_Time"></param>
        public Download_Data_Progress_EventArgs(long Received_Bytes_Current, long Received_Bytes_Total, decimal Calulated_Current_Divide_Total, int Calulated_Percentage, string Received_File_Name, DateTime Received_Start_Time)
        {
            this.Bytes_To_Receive_Total = Received_Bytes_Total;
            this.Bytes_Received = Received_Bytes_Current;
            this.Download_Percentage = Calulated_Percentage;
            this.Start_Time = Received_Start_Time;
            this.Bytes_Received_Over_Total = Calulated_Current_Divide_Total;
            this.File_Name = Received_File_Name;
        }
    }
}
