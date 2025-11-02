namespace SBRW.Launcher.Core.Downloader.Models_
{
    /// <summary>
    /// Reports the state of the verification process.
    /// </summary>
    public class Download_Raw_Progress_Model
    {
        /// <summary>
        /// A human-readable status message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Generic status message
        /// </summary>
        public Download_Raw_Status Status { get; set; } = Download_Raw_Status.Idle;
        /// <summary>
        /// The current file being processed (scanned or downloaded).
        /// </summary>
        public string? CurrentFile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long CurrentFileNumber { get; set; }
        /// <summary>
        /// Overall progress percentage (0-100) for the current major step.
        /// </summary>
        public int Percentage { get; set; }
        /// <summary>
        /// For downloads, the bytes received for the current file.
        /// </summary>
        public long BytesDownloaded { get; set; }
        /// <summary>
        /// For downloads, the total bytes of the current file.
        /// </summary>
        public long TotalBytesToDownload { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long TotalFileNumber { get; set; }
    }
}