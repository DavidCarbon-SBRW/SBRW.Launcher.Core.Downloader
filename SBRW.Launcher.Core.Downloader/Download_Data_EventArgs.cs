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
        public int Completed_Percentage { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string Download_State { get; internal set; } = string.Empty;
        private long File_Size_Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Current { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Currente"></param>
        public Download_Data_EventArgs(long Received_File_Size_Total, long Received_File_Size_Currente)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Currente;
            this.Completed_Percentage = (int)((((double)Received_File_Size_Currente) / File_Size_Total) * 100);
        }
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
            this.Completed_Percentage = Received_Completed_Percentage;
            this.Download_State = Received_Download_State;
        }
    }
}