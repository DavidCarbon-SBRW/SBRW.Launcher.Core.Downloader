using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SBRW.Launcher.Core.Downloader.Exception_
{
    /// <summary>
    /// 
    /// </summary>
    public class Downloaded_File_Hash_Invalid_Exception : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Error_Message"></param>
        public Downloaded_File_Hash_Invalid_Exception(string Error_Message) : base(Error_Message)
        { 

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Error_Message"></param>
        /// <param name="Error"></param>
        public Downloaded_File_Hash_Invalid_Exception(string Error_Message, Exception Error) : base(Error_Message, Error) 
        {

        }
    }
}
