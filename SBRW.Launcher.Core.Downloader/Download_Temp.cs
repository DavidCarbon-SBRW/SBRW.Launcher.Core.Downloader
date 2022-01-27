using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SBRW.Launcher.Core.Downloader
{
    internal class Download_Temp
    {
        /// <summary>
        /// 
        /// </summary>
        public static string? Set_File_Name { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public static string? Set_Full_Path { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Web_Address"></param>
        /// <param name="Location_File_Path"></param>
        /// <returns></returns>
        public Download_Temp Create(string Web_Address, string Location_File_Path)
        {
            return Create(-1, Web_Address, Location_File_Path, null, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Local_Web_Proxy"></param>
        /// <returns></returns>
        public Download_Temp Create(long Provided_File_Size, string Web_Address, string Location_Folder, IWebProxy? Local_Web_Proxy = null)
        {
            return Create(Provided_File_Size, Web_Address, Location_Folder, null, Local_Web_Proxy);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Provided_File_Size"></param>
        /// <param name="Web_Address"></param>
        /// <param name="Location_Folder"></param>
        /// <param name="Provided_File_Name"></param>
        /// <param name="Local_Web_Proxy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Download_Temp Create(long Provided_File_Size, string Web_Address, string Location_Folder, string? Provided_File_Name = null, IWebProxy? Local_Web_Proxy = null)
        {
            // This is what we will return
            Download_Client Data_Recevied = new Download_Client();
            if (Local_Web_Proxy != null)
            {
                Data_Recevied.Web_Proxy = Local_Web_Proxy;
            }

            long Size_Recevied = Data_Recevied.GetFileSize(Web_Address, Provided_File_Size);
            Data_Recevied.Data_Size = Size_Recevied;

            WebRequest Data_Request = Data_Recevied.GetRequest(Web_Address);
            try
            {
                Data_Recevied.Web_Response = Data_Request.GetResponse();
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format(
                    "Error downloading \"{0}\": {1}", Web_Address, e.Message), e);
            }

            // Check to make sure the response isn't an error. If it is this method
            // will throw exceptions.
            Data_Recevied.ValidateResponse(Data_Recevied.Web_Response, Web_Address);

            // Take the name of the file given to use from the web server.
            Set_File_Name = Path.GetFileName(!string.IsNullOrWhiteSpace(Provided_File_Name) ? Provided_File_Name : Data_Recevied.Web_Response.ResponseUri.ToString());
            Set_Full_Path = Path.Combine(Location_Folder, ".Launcher", "Downloads", Set_File_Name);

            // If we don't know how big the file is supposed to be,
            // we can't resume, so delete what we already have if something is on disk already.
            if (!Data_Recevied.IsProgressKnown && File.Exists(Set_Full_Path))
            {
                File.Delete(Set_Full_Path);
            }

            if (Data_Recevied.IsProgressKnown && File.Exists(Set_Full_Path))
            {
                // We only support resuming on http Data_Requestuests
                if (!(Data_Recevied.Web_Response is HttpWebResponse))
                {
                    File.Delete(Set_Full_Path);
                }
                else
                {
                    // Try and start where the file on disk left off
                    Data_Recevied.Data_Start = new FileInfo(Set_Full_Path).Length;

                    // If we have a file that's bigger than what is online, then something 
                    // strange happened. Delete it and start again.
                    if (Data_Recevied.Data_Start > Size_Recevied)
                    {
                        File.Delete(Set_Full_Path);
                    }
                    else if (Data_Recevied.Data_Start < Size_Recevied)
                    {
                        // Try and resume by creating a new Data_Requestuest with a new start position
                        Data_Recevied.Web_Response.Close();
                        Data_Request = Data_Recevied.GetRequest(Web_Address);
                        ((HttpWebRequest)Data_Request).AddRange((int)Data_Recevied.Data_Start);
                        ((HttpWebRequest)Data_Request).Headers["X-MTNTR-HEADER-VAL"] = (Data_Recevied.Data_Start).ToString();
                        ((HttpWebRequest)Data_Request).Headers["X-UserAgent"] = Download_Data_Support.Header;
                        Data_Recevied.Web_Response = Data_Request.GetResponse();

                        if (((HttpWebResponse)Data_Recevied.Web_Response).StatusCode != HttpStatusCode.PartialContent)
                        {
                            // They didn't support our resume request
                            File.Delete(Set_Full_Path);
                            Data_Recevied.Data_Start = 0;
                        }
                    }
                }
            }

            return Data_Recevied;
        }
    }
}
