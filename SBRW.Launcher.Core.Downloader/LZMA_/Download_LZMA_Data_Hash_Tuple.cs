using System;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    internal class Download_LZMA_Data_Hash_Tuple
    {
        /// <summary>
        /// 
        /// </summary>
        public string Old { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string New { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Ticks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldHash"></param>
        /// <param name="newHash"></param>
        /// <param name="ticks"></param>
        public Download_LZMA_Data_Hash_Tuple(string oldHash, string newHash, long ticks)
        {
            this.Old = oldHash;
            this.New = newHash;
            this.Ticks = ticks;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldHash"></param>
        /// <param name="newHash"></param>
        public Download_LZMA_Data_Hash_Tuple(string oldHash, string newHash) : this(oldHash, newHash, DateTime.Now.AddYears(1).Ticks)
        {
        }
    }
}
