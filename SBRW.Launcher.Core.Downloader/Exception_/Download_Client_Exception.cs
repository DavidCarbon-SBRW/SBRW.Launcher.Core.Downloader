using System;

namespace SBRW.Launcher.Core.Downloader.Exception_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Client_Exception : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Error_Message"></param>
        public Download_Client_Exception(string Error_Message) : base(Error_Message)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Error_Message"></param>
        /// <param name="Error"></param>
        public Download_Client_Exception(string Error_Message, Exception Error) : base(Error_Message, Error)
        {

        }
    }
}
