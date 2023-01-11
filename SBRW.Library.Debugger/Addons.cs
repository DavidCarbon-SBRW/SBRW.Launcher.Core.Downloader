using System;

namespace SBRW.Library.Debugger
{
    internal static class Addons
    {
        public static string FormatFileSize(this long byteCount, bool si)
        {
            try
            {
                int num = (si ? 1000 : 1024);
                if (byteCount < num)
                {
                    return byteCount + " B";
                }

                int num2 = (int)(Math.Log(byteCount) / Math.Log(num));
                string arg = (si ? "kMGTPE" : "KMGTPE")[num2 - 1] + (si ? "" : "i");
                return string.Format("{0}{1}B", Convert.ToDecimal((double)byteCount / Math.Pow(num, num2)).ToString("0.00"), arg);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
