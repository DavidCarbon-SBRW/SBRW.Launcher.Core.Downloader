using System;
using System.Runtime.Serialization;

namespace SBRW.Launcher.Core.Downloader.LZMA
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Download_LZMA_Exception_Uncompression : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public int Error_Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Recevied_Error_Code"></param>
        public Download_LZMA_Exception_Uncompression(int Recevied_Error_Code)
        {
            this.Error_Code = Recevied_Error_Code;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Recevied_Error_Code"></param>
        /// <param name="Exception_Message"></param>
        public Download_LZMA_Exception_Uncompression(int Recevied_Error_Code, string Exception_Message) : base(Exception_Message)
        {
            this.Error_Code = Recevied_Error_Code;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Recevied_Error_Code"></param>
        /// <param name="Exception_Message"></param>
        /// <param name="Recevied_Exception"></param>
        public Download_LZMA_Exception_Uncompression(int Recevied_Error_Code, string Exception_Message, Exception Recevied_Exception) : base(Exception_Message, Recevied_Exception)
        {
            this.Error_Code = Recevied_Error_Code;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object_Information"></param>
        /// <param name="Live_Context"></param>
        protected Download_LZMA_Exception_Uncompression(SerializationInfo Object_Information, StreamingContext Live_Context) : base(Object_Information, Live_Context)
        {

        }
    }
}
