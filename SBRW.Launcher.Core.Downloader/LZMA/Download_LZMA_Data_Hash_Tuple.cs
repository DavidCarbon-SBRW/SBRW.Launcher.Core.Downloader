using System;

namespace SBRW.Launcher.Core.Downloader.LZMA
{
    internal class Download_LZMA_Data_Hash_Tuple
    {
        public string Old { get; set; }
        public string New { get; set; }
        public long Ticks { get; set; }

        public Download_LZMA_Data_Hash_Tuple(string oldHash, string newHash, long ticks)
        {
            this.Old = oldHash;
            this.New = newHash;
            this.Ticks = ticks;
        }

        public Download_LZMA_Data_Hash_Tuple(string oldHash, string newHash) : this(oldHash, newHash, DateTime.Now.AddYears(1).Ticks)
        {
        }
    }
}
