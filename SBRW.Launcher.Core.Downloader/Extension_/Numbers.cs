namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// Downloader Related Functions
    /// </summary>
    /// <remarks>Aimed to Pass Fail Safe Checks</remarks>
    public class Numbers
    {
        /// <summary>
        /// FailSafe Percent Value Check
        /// </summary>
        /// <param name="Provided_Value"></param>
        /// <param name="Multiply_By"></param>
        /// <remarks>Math Equation Example: (30/60)*100 = 50</remarks>
        /// <returns></returns>
        public static int Download_Percentage_Check(long Provided_Value, int Multiply_By = 100)
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
                if (Multiply_By <= 0 || Multiply_By >= 100)
                {
                    Multiply_By = 100;
                }

                return (int)Provided_Value * Multiply_By;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Numerator"></param>
        /// <param name="Denominator"></param>
        /// <returns></returns>
        public static long Division_Check(long Numerator, long Denominator)
        {
            if (Denominator <= 0)
            {
                return -1;
            }
            else
            {
                return Numerator / Denominator;
            }
        }
    }
}
