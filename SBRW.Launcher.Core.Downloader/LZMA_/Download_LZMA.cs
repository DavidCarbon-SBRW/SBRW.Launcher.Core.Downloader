using System;
using System.Runtime.InteropServices;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_LZMA
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="destLen"></param>
        /// <param name="src"></param>
        /// <param name="srcLen"></param>
        /// <param name="outProps"></param>
        /// <param name="outPropsSize"></param>
        /// <returns></returns>
        [DllImport("LZMA.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern int LzmaUncompress(byte[] dest, ref IntPtr destLen, byte[] src, ref IntPtr srcLen, byte[] outProps, IntPtr outPropsSize);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destFile"></param>
        /// <param name="destLen"></param>
        /// <param name="src"></param>
        /// <param name="srcLen"></param>
        /// <param name="outProps"></param>
        /// <param name="outPropsSize"></param>
        /// <returns></returns>
        [DllImport("LZMA.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern int LzmaUncompressBuf2File(string destFile, ref IntPtr destLen, byte[] src, ref IntPtr srcLen, byte[] outProps, IntPtr outPropsSize);
    }
}