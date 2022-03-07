namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Download_LZMA_Command
    {
        /// <summary>
        /// 
        /// </summary>
        public Download_LZMA_Data Cached_Data { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloader"></param>
        protected Download_LZMA_Command(Download_LZMA_Data downloader)
        {
            this.Cached_Data = downloader;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public abstract void Execute(object[] parameters);
    }
}