using SBRW.Launcher.Core.Downloader.EventArg_;
using SBRW.Launcher.Core.Downloader.Exception_;
using SBRW.Launcher.Core.Downloader.Extension_;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SBRW.Launcher.Core.Downloader
{

    /// <summary>
    /// 
    /// </summary>
    public class Download_Extract
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Extract_Information? Extract_Status_Information { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Extract_Information? Extract_Status() { return Extract_Status_Information; }
        /// <summary>
        /// 
        /// </summary>
        public bool Disable_Extract_Status_Information { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private string Current_File { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string File_Extension_Replacement { get; set; } = ".sbrw";
        /// <summary>
        /// 
        /// </summary>
        public string File_Temporary_Location { get; set; } = Path.GetTempFileName();
        /// <summary>
        /// 
        /// </summary>
        private int Total_Current_File { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private int Total_File { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private DateTime Start_Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Extract_Progress_Handler(object Sender, Download_Extract_Progress_EventArgs Events);
        /// <summary>
        /// 
        /// </summary>
        public event Download_Extract_Progress_Handler? Live_Progress;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Extract_Completion_Handler(object Sender, Download_Extract_Complete_EventArgs Events);
        /// <summary>
        /// 
        /// </summary>
        public event Download_Extract_Completion_Handler? Complete;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Events"></param>
        public delegate void Download_Extract_Exception_Handler(object Sender, Download_Exception_EventArgs Events);
        /// <summary>
        /// 
        /// </summary>
        public event Download_Extract_Exception_Handler? Internal_Error;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Event_Hook"></param>
        /// <param name="Exception_Caught"></param>
        internal void Exception_Router(bool Event_Hook, Exception Exception_Caught)
        {
            if (this.Internal_Error != null && Event_Hook)
            {
                this.Internal_Error(this, new Download_Exception_EventArgs(Exception_Caught, DateTime.Now));
            }
            else
            {
                throw Exception_Caught;
            }
        }
        /// <summary>
        /// Extracts a Custom Pack File
        /// </summary>
        /// <param name="File_Custom_Pack_Path">Pack File Location Path</param>
        /// <param name="File_Extract_Path">File Extraction Path</param>
        public void Custom_Unpack(string File_Custom_Pack_Path, string File_Extract_Path)
        {
            if (string.IsNullOrWhiteSpace(File_Custom_Pack_Path) || string.IsNullOrWhiteSpace(File_Extract_Path))
            {
                Exception_Router(true, new ArgumentNullException());
            }
            else
            {
                Start_Time = DateTime.Now;

#pragma warning disable IDE0063 // Use simple 'using' statement
                using (ZipArchive Package_Archive = ZipFile.OpenRead(File_Custom_Pack_Path))
                {
                    Total_File = Package_Archive.Entries.Count;

                    foreach (ZipArchiveEntry Package_File in Package_Archive.Entries)
                    {
                        if (Cancel)
                        {
                            break;
                        }
                        else
                        {
                            Current_File = Package_File.FullName;
                            Total_Current_File++;

                            if (!File.Exists(Path.Combine(File_Extract_Path, Current_File.Replace(File_Extension_Replacement, string.Empty))) && !Cancel)
                            {
                                if ((Current_File.Substring(Current_File.Length - 1) == "/") && !Cancel)
                                {
                                    /* Is a directory, create it! */
                                    string Directory_Name = Path.Combine(File_Extract_Path, Current_File.Remove(Current_File.Length - 1));
                                    try
                                    {
                                        if (Directory.Exists(Directory_Name))
                                        {
                                            Directory.Delete(Directory_Name, true);
                                        }
                                    }
                                    catch (Exception Error)
                                    {
                                        Exception_Router(true, Error);
                                    }

                                    try
                                    {
                                        if (!Directory.Exists(Directory_Name))
                                        {
                                            Directory.CreateDirectory(Directory_Name);
                                        }
                                    }
                                    catch (Exception Error)
                                    {
                                        Exception_Router(true, Error);
                                    }
                                }
                                else if (!Cancel)
                                {
                                    try
                                    {
                                        string File_Name_Decrypt = Current_File.Replace(File_Extension_Replacement, string.Empty);
                                        string[] File_Name_Split = File_Name_Decrypt.Split('/');
                                        string File_Name_Last_Check = string.Empty;

                                        if (File_Name_Split.Length >= 2)
                                        {
                                            File_Name_Last_Check = Path.Combine(File_Name_Split[File_Name_Split.Length - 2], File_Name_Split[File_Name_Split.Length - 1]);
                                        }
                                        else
                                        {
                                            File_Name_Last_Check = File_Name_Split.Last();
                                        }

                                        string KEY = Regex.Replace(Hashes.Hash_String(1, File_Name_Last_Check), "[^0-9.]", string.Empty).Substring(0, 8);
                                        string IV = Regex.Replace(Hashes.Hash_String(0, File_Name_Last_Check), "[^0-9.]", string.Empty).Substring(0, 8);

                                        Package_File.ExtractToFile(File_Temporary_Location, true);
#if !NETFRAMEWORK
#pragma warning disable SYSLIB0021 // Type or member is obsolete
#endif
                                        DESCryptoServiceProvider Crypto_Provider = new DESCryptoServiceProvider()
                                        {
                                            Key = Encoding.ASCII.GetBytes(KEY),
                                            IV = Encoding.ASCII.GetBytes(IV)
                                        };
#if !NETFRAMEWORK
#pragma warning restore SYSLIB0021 // Type or member is obsolete
#endif

                                        FileStream File_Stream = new FileStream(Path.Combine(File_Extract_Path, File_Name_Decrypt), FileMode.Create);
                                        CryptoStream Decrypt_Stream = new CryptoStream(File_Stream, Crypto_Provider.CreateDecryptor(), CryptoStreamMode.Write);
                                        BinaryWriter Binary_File = new BinaryWriter(Decrypt_Stream);

                                        using (BinaryReader Binary_Reader = new BinaryReader(File.Open(File_Temporary_Location, FileMode.Open)))
                                        {
                                            long numBytes = new FileInfo(File_Temporary_Location).Length;
                                            Binary_File.Write(Binary_Reader.ReadBytes((int)numBytes));
                                        }

                                        Binary_File.Flush();
                                        Binary_File.Close();
                                        Binary_File.Dispose();
                                        Decrypt_Stream.Flush();
                                        Decrypt_Stream.Close();
                                        Decrypt_Stream.Dispose();
                                        File_Stream.Close();
                                        File_Stream.Dispose();
                                        Crypto_Provider.Clear();
                                        Crypto_Provider.Dispose();
                                    }
                                    catch (Exception Error)
                                    {
                                        Exception_Router(true, Error);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (Cancel)
                            {
                                break;
                            }

                            if (!Disable_Extract_Status_Information && !Cancel)
                            {
                                Extract_Status_Information = new Extract_Information()
                                {
                                    Extract_Percentage = 100 * Total_Current_File / Total_File,
                                    File_Current_Name = Current_File,
                                    File_Total = Total_File,
                                    File_Current = Total_Current_File,
                                    Start_Time = Start_Time
                                };
                            }

                            if ((this.Live_Progress != null) && !Cancel)
                            {
                                this.Live_Progress(this, new Download_Extract_Progress_EventArgs(100 * Total_Current_File / Total_File, Current_File, Total_File, Total_Current_File, Start_Time));
                            }

                            if ((Total_Current_File == Total_File) && !Cancel)
                            {
                                if (!Disable_Extract_Status_Information && !Cancel)
                                {
                                    Extract_Status_Information = new Extract_Information()
                                    {
                                        Extract_Percentage = 100 * Total_Current_File / Total_File,
                                        File_Current_Name = Current_File,
                                        File_Total = Total_File,
                                        File_Current = Total_Current_File,
                                        Start_Time = Start_Time,
                                        End_Time = DateTime.Now,
                                        Extract_Complete = true
                                    };
                                }

                                if (this.Complete != null)
                                {
                                    this.Complete(this, new Download_Extract_Complete_EventArgs(true, DateTime.Now));
                                }
                                
                                Cancel = true;
                            }
                            else if (Cancel)
                            {
                                break;
                            }
                        }
                    }
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }
        }
    }
}
