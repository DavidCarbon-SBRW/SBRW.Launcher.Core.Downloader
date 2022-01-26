using System;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// Progress of a downloading file.
    /// </summary>
    public class Download_Data_EventArgs : EventArgs
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
        public long File_Size_Current { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Start_Time { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_Download_State"></param>
        public Download_Data_EventArgs(string Received_Download_State)
        {
            this.Download_State = Received_Download_State;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_Completed_Percentage"></param>
        /// <param name="Received_Download_State"></param>
        public Download_Data_EventArgs(int Received_Completed_Percentage, string Received_Download_State)
        {
            this.Download_Percentage = Received_Completed_Percentage;
            this.Download_State = Received_Download_State;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Current"></param>
        public Download_Data_EventArgs(long Received_File_Size_Total, long Received_File_Size_Current)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Current;
            this.Download_Percentage = (int)((((double)Received_File_Size_Current) / Received_File_Size_Total) * 100);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Current"></param>
        /// <param name="Received_Start_Time"></param>
        public Download_Data_EventArgs(long Received_File_Size_Total, long Received_File_Size_Current, DateTime Received_Start_Time)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Current;
            this.Download_Percentage = (int)((((double)Received_File_Size_Current) / Received_File_Size_Total) * 100);
            this.Start_Time = Received_Start_Time;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Current"></param>
        /// <param name="Received_Start_Time"></param>
        /// <param name="Received_Download_State"></param>
        public Download_Data_EventArgs(long Received_File_Size_Total, long Received_File_Size_Current, DateTime Received_Start_Time, string Received_Download_State)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Current;
            this.Download_Percentage = (int)((((double)Received_File_Size_Current) / Received_File_Size_Total) * 100);
            this.Start_Time = Received_Start_Time;
            this.Download_State = Received_Download_State;
        }
    }
}