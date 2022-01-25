using System;
using System.Runtime.Serialization;

namespace SBRW.Launcher.Core.Downloader.LZMA
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Download_LZMA_Exception : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public Download_LZMA_Exception()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Exception_Message"></param>
        public Download_LZMA_Exception(string Exception_Message) : base(Exception_Message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Exception_Message"></param>
        /// <param name="Recevied_Exception"></param>
        public Download_LZMA_Exception(string Exception_Message, Exception Recevied_Exception) : base(Exception_Message, Recevied_Exception)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object_Information"></param>
        /// <param name="Live_Context"></param>
        protected Download_LZMA_Exception(SerializationInfo Object_Information, StreamingContext Live_Context) : base(Object_Information, Live_Context)
        {
        }
    }
}