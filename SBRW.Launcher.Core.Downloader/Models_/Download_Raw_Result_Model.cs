using System.Collections.Generic;

namespace SBRW.Launcher.Core.Downloader.Models_
{
    /// <summary>
    /// A summary of the completed verification process.
    /// </summary>
    public class Download_Raw_Result_Model
    {
        /// <summary>
        /// 
        /// </summary>
        public int TotalFilesScanned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalFilesMissingOrInvalid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FilesSuccessfullyDownloaded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FilesFailedToDownload { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool WasCancelled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> FailedDownloadFiles { get; } = new List<string>();
    }
}