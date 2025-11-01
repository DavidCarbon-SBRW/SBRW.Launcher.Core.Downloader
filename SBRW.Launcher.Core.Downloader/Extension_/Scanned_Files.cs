using System.IO;

namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// 
    /// </summary>
    public class Scanned_Files
    {
        /// <summary>
        /// 
        /// </summary>
        public FileInfo? File_Info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public FileSystemInfo? File_System_Info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DirectoryInfo? Directory_Info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Skip { get; set; }
    }
}
