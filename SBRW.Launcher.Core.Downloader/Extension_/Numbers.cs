namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// Downloader Related Functions
    /// </summary>
    /// <remarks>Aimed to Pass Fail Safe Checks</remarks>
    public class Numbers
    {
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
                return Numerator/Denominator;
            }
        }
    }
}
