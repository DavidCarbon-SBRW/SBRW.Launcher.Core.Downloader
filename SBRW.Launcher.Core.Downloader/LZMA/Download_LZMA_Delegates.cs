using System;

namespace SBRW.Launcher.Core.Downloader.LZMA
{
    /// <summary>
    /// 
    /// </summary>
    public static class Download_LZMA_Delegates
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void Download_LZMA_Finished();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        public delegate void Download_LZMA_Failed(Exception ex);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dowloadLength"></param>
        /// <param name="downloadCurrent"></param>
        /// <param name="compressedLength"></param>
        /// <param name="fileName"></param>
        /// <param name="skipdownload"></param>
        public delegate void Download_LZMA_Progress_Updated(long dowloadLength, long downloadCurrent, long compressedLength, string fileName, int skipdownload = 0);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="currentCount"></param>
        /// <param name="allFilesCount"></param>
        public delegate void Download_LZMA_Show_Extract(string filename, long currentCount, long allFilesCount);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="header"></param>
        public delegate void Download_LZMA_Show_Message(string message, string header);
    }
}
