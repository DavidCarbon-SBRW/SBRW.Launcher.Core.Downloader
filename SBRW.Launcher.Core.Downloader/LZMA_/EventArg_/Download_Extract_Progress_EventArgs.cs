using System;

namespace SBRW.Launcher.Core.Downloader.LZMA_.EventArg_
{
    /// <summary>
    /// Progress of a downloading file.
    /// </summary>
    public class Download_Extract_Progress_EventArgs : EventArgs
    {
        /// <summary>
        /// Percentage of the Extraction Processs
        /// </summary>
        public int Extract_Percentage { get; internal set; }
        /// <summary>
        /// Total amount of Files
        /// </summary>
        public long File_Total { get; internal set; }
        /// <summary>
        /// Current Count out of the Toal amount of Files
        /// </summary>
        public long File_Current { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Current_Divide_Total { get; internal set; }
        /// <summary>
        /// Current File Name
        /// </summary>
        public string? File_Current_Name { get; internal set; }
        /// <summary>
        /// Extraction Start Time
        /// </summary>
        public DateTime Start_Time { get; internal set; }
        /// <summary>
        /// FailSafe Percent Value Check
        /// </summary>
        /// <param name="Provided_Value"></param>
        /// <returns></returns>
        private int Download_Percentage_Check(long Provided_Value)
        {
            if (Provided_Value <= 0)
            {
                return 0;
            }
            else if (Provided_Value >= 100)
            {
                return 100;
            }
            else
            {
                return (int)Provided_Value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Numerator"></param>
        /// <param name="Denominator"></param>
        /// <returns></returns>
        private long Division_Check(long Numerator, long Denominator)
        {
            if (Denominator <= 0)
            {
                return -1;
            }
            else
            {
                return (long)decimal.Divide(Numerator, Denominator);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Received_Extract_File_Name">Current File Name</param>
        /// <param name="Received_File_Total">Total amount of Files</param>
        /// <param name="Received_File_Current">Current Count out of the Toal amount of Files</param>
        /// <param name="Received_Time">Extraction Start Time</param>
        public Download_Extract_Progress_EventArgs(string Received_Extract_File_Name, long Received_File_Total, long Received_File_Current, DateTime Received_Time)
        {
            this.Extract_Percentage = Download_Percentage_Check(Division_Check(Received_File_Current, Received_File_Total) * 100);
            this.File_Current_Name = Received_Extract_File_Name;
            this.File_Total = Received_File_Total;
            this.File_Current = Received_File_Current;
            this.File_Current_Divide_Total = Division_Check(Received_File_Current, Received_File_Total);
            this.Start_Time = Received_Time;
        }
    }
}
