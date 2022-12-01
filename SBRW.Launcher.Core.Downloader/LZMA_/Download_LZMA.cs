using System;
using System.Runtime.InteropServices;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public static class Download_LZMA
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
        [DllImport("LZMA.dll", EntryPoint = "LzmaUncompress", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int LzmaUncompress_32([In, Out] byte[] dest, [In, Out] IntPtr destLen, [In, Out] byte[] src, [In, Out] IntPtr srcLen, [In, Out] byte[] outProps, [In, Out] IntPtr outPropsSize);
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
        [DllImport("LZMA_X64.dll", EntryPoint = "LzmaUncompress", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int LzmaUncompress_64([In, Out] byte[] dest, [In, Out] IntPtr destLen, [In, Out] byte[] src, [In, Out] IntPtr srcLen, [In, Out] byte[] outProps, [In, Out] IntPtr outPropsSize);
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
        [DllImport("LZMA.dll", EntryPoint = "LzmaUncompressBuf2File", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int LzmaUncompressBuf2File_32([In, Out] string destFile, [In, Out] IntPtr destLen, [In, Out] byte[] src, [In, Out] IntPtr srcLen, byte[] outProps, [In, Out] IntPtr outPropsSize);
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
        [DllImport("LZMA_X64.dll", EntryPoint = "LzmaUncompressBuf2File", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int LzmaUncompressBuf2File_64([In, Out] string destFile, [In, Out] IntPtr destLen, [In, Out] byte[] src, [In, Out] IntPtr srcLen, byte[] outProps, [In, Out] IntPtr outPropsSize);
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
        public static int LzmaUncompress(byte[] dest, IntPtr destLen, byte[] src, IntPtr srcLen, byte[] outProps, IntPtr outPropsSize)
        {
            if (Environment.Is64BitProcess)
            {
                return LzmaUncompress_64(dest, destLen, src, srcLen, outProps, outPropsSize);
            }
            else
            {
                return LzmaUncompress_32(dest, destLen, src, srcLen, outProps, outPropsSize);
            }
        }
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
        public static int LzmaUncompressBuf2File(string destFile, IntPtr destLen, byte[] src, IntPtr srcLen, byte[] outProps, IntPtr outPropsSize)
        {
            if (Environment.Is64BitProcess)
            {
                return LzmaUncompressBuf2File_64(destFile, destLen, src, srcLen, outProps, outPropsSize);
            }
            else
            {
                return LzmaUncompressBuf2File_32(destFile, destLen, src, srcLen, outProps, outPropsSize);
            }
        }
    }
}