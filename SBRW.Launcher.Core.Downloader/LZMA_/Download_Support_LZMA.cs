using System;
using System.Runtime.Serialization;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Support_LZMA
    {
        /// <summary>
        /// 
        /// </summary>
        public static string Speech_Language { get; set; } = "en";
        /// <summary>
        /// Check System Language and Return Current Lang for Speech Files
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        public static string SpeechFiles(string Language = "")
        {
            string CurrentLang = (!string.IsNullOrWhiteSpace(Language)) ? Language.ToLower() : string.Empty;
            if (CurrentLang == "eng") Speech_Language = "en";
            else if (CurrentLang == "ger" || CurrentLang == "deu") Speech_Language = "de";
            else if (CurrentLang == "rus") Speech_Language = "ru";
            else if (CurrentLang == "spa") Speech_Language = "es";
            else Speech_Language = "en";

            return Speech_Language;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int SpeechFilesSize()
        {
            string CurrentLang = SpeechFiles();

            if (CurrentLang == "eng" || CurrentLang == "en") return 141805935;
            else if (CurrentLang == "ger" || CurrentLang == "deu" || CurrentLang == "de") return 105948386;
            else if (CurrentLang == "rus" || CurrentLang == "ru") return 121367723;
            else if (CurrentLang == "spa" || CurrentLang == "es") return 101540466;
            else return 141805935;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public delegate void DownloadFinished();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ex"></param>
    public delegate void DownloadFailed(Exception ex);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dowloadLength"></param>
    /// <param name="downloadCurrent"></param>
    /// <param name="compressedLength"></param>
    /// <param name="fileName"></param>
    /// <param name="skipdownload"></param>
    public delegate void ProgressUpdated(long dowloadLength, long downloadCurrent, long compressedLength, string fileName, int skipdownload = 0);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="currentCount"></param>
    /// <param name="allFilesCount"></param>
    public delegate void ShowExtract(string filename, long currentCount, long allFilesCount);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="header"></param>
    public delegate void ShowMessage(string message, string header);
    /// <summary>
    /// 
    /// </summary>
    public abstract class DownloaderCommand
    {
        /// <summary>
        /// 
        /// </summary>
        protected Download_LZMA _downloader;
        /// <summary>
        /// 
        /// </summary>
        public Download_LZMA Downloader
        {
            get { return this._downloader; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloader"></param>
        protected DownloaderCommand(Download_LZMA downloader)
        {
            this._downloader = downloader;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public abstract void Execute(object[] parameters);
    }
    /// <summary>
    /// 
    /// </summary>
    public class VerifyCommand : DownloaderCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloader"></param>
        public VerifyCommand(Download_LZMA downloader) : base(downloader)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public override void Execute(object[] parameters)
        {
            this._downloader.StartVerification((string)parameters[0], (string)parameters[1], (string)parameters[2], (bool)parameters[3], (bool)parameters[4], (bool)parameters[5]);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Download_Client_LZMA_Exception : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public Download_Client_LZMA_Exception()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public Download_Client_LZMA_Exception(string message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public Download_Client_LZMA_Exception(string message, Exception innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected Download_Client_LZMA_Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Download_Client_LZMA_Uncompression_Exception : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public int mErrorCode;
        /// <summary>
        /// 
        /// </summary>
        public int ErrorCode
        {
            get { return this.mErrorCode; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        public Download_Client_LZMA_Uncompression_Exception(int errorCode)
        {
            this.mErrorCode = errorCode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public Download_Client_LZMA_Uncompression_Exception(int errorCode, string message) : base(message)
        {
            this.mErrorCode = errorCode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public Download_Client_LZMA_Uncompression_Exception(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            this.mErrorCode = errorCode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected Download_Client_LZMA_Uncompression_Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
