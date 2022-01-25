namespace SBRW.Launcher.Core.Downloader.LZMA
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_LZMA_Command_Verify : Download_LZMA_Command
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Cached_Data"></param>
        public Download_LZMA_Command_Verify(Download_LZMA_Data Cached_Data) : base(Cached_Data)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public override void Execute(object[] parameters)
        {
            this.Cached_Data.StartVerification((string)parameters[0], (string)parameters[1], (string)parameters[2], (bool)parameters[3], (bool)parameters[4], (bool)parameters[5]);
        }
    }
}
