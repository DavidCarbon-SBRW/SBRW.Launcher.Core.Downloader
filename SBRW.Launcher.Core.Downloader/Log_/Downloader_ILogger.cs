using System;

namespace SBRW.Launcher.Core.Downloader.Log_
{
    /// <summary>
    /// An interface for logging, allowing the consumer to hook in their own logging framework.
    /// </summary>
    public interface Downloader_ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Error(string message, Exception? ex = null);
    }
}
