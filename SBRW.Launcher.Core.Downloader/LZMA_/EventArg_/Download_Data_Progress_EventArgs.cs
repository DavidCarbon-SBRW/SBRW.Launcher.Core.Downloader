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
        public long File_Size_Current { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long File_Size_Current_Divide_Total { get; internal set; }
        /// <summary>
        /// 
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
        /// <param name="Received_File_Size_Total"></param>
        /// <param name="Received_File_Size_Current"></param>
        /// <param name="Received_Start_Time"></param>
        public Download_Data_Progress_EventArgs(long Received_File_Size_Total, long Received_File_Size_Current, DateTime Received_Start_Time)
        {
            this.File_Size_Total = Received_File_Size_Total;
            this.File_Size_Current = Received_File_Size_Current;
            this.Download_Percentage = Download_Percentage_Check(Division_Check(Received_File_Size_Current, Received_File_Size_Total) * 100);
            this.Start_Time = Received_Start_Time;
            this.File_Size_Current_Divide_Total = Division_Check(Received_File_Size_Current, Received_File_Size_Total);
        }
    }
}
