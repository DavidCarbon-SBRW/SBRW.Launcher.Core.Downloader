using System;

namespace SBRW.Launcher.Core.Downloader.Log_
{
    /// <summary>
    /// An interface for logging, allowing the consumer to hook in their own logging framework.
    /// </summary>
    public interface Downloader_ILogger
    {
        /// <summary>
        /// Used for Informational Details
        /// </summary>
        /// <param name="message">Log Message</param>
        void Info(string message);
        /// <summary>
        /// Used for Invalid Entries
        /// </summary>
        /// <param name="message">Log Message</param>
        void Invalid(string message);
        /// <summary>
        /// Used for Deleted Entries
        /// </summary>
        /// <param name="message">Log Message</param>
        void Deleted(string message);
        /// <summary>
        /// Used for Missing Entries
        /// </summary>
        /// <param name="message">Log Message</param>
        void Missing(string message);
        /// <summary>
        /// Used for Downloaded Entries
        /// </summary>
        /// <param name="message">Log Message</param>
        void Downloaded(string message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);
        /// <summary>
        /// Used for Error Event Details
        /// </summary>
        /// <param name="message">Log Message</param>
        /// <param name="Error">Exception Message</param>
        void Error(string message, Exception? Error = null);
    }
}
