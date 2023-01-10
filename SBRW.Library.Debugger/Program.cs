using SBRW.Launcher.Core.Downloader;
using System;
using System.IO;
using System.Net;
using System.Reflection.Emit;

namespace SBRW.Library.Debugger
{
    internal class Program
    {
        public static Download_Queue? Pack_SBRW_Downloader { get; set; }
        public static Download_Extract? Pack_SBRW_Unpacker { get; set; }
        private static int Pack_SBRW_Downloader_Time_Span { get; set; }
        public static bool Pack_SBRW_Downloader_Unpack_Lock { get; set; }

        private static string GameFolderPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles"); } }
        private static string GamePackPath { get { return Path.Combine(GameFolderPath, ".Launcher", "Downloads", "GameFiles.sbrwpack"); } }
        private static string Launcher_CDN { get; set; } = "http://g2-sbrw.davidcarbon.download";

        static void Main(string[] args)
        {
            if (!Directory.Exists(GameFolderPath))
            {
                Directory.CreateDirectory(GameFolderPath);
            }

            Console.WriteLine("Hello, World!");
            Game_Pack_Downloader();
        }

        private static void Game_Pack_Downloader()
        {
            Console.WriteLine("Loading".ToUpper());

            long Game_Folder_Size = File_and_Folder_Extention.GetDirectorySize_GameFiles(new DirectoryInfo(GameFolderPath));
            /* TODO: Check for other files and Folder Size */
            if ((Game_Folder_Size == -1))
            {
                Game_Folder_Size = 3295097405;
            }

            if (!File.Exists(Path.Combine(GameFolderPath, "nfsw.exe")) &&
                Game_Folder_Size <= 3295097404)
            {
                Console.WriteLine("Downloading: Core Game Files Package".ToUpper());

                // Get the size of the local file.
                long localFileSize = 0;
                if (File.Exists(GameFolderPath))
                {
                    FileInfo info = new FileInfo(GameFolderPath);
                    localFileSize = info.Length;
                }

                // Create a new HttpWebRequest instance.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Launcher_CDN + "/GameFiles.sbrwpack");

                // Set the start point for the download.
                request.AddRange(localFileSize);

                // Read the file in chunks of 1KB.
                int chunkSize = 1024;
                byte[] buffer = new byte[chunkSize];
                int bytesRead;

                // Send the request and get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing the response.
                    using (Stream stream = response.GetResponseStream())
                    {
                        // Use a BinaryReader to read the file in small chunks.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            // Use a FileStream to write the file to the local file system.
                            using (FileStream writer = new FileStream(GamePackPath, FileMode.Append))
                            {
                                
                                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    // Write the chunk to the local file.
                                    writer.Write(buffer, 0, bytesRead);

                                    // Calculate the download progress.
                                    double progress = (localFileSize + writer.Length) / (double)response.ContentLength;

                                    // Show the percentage downloaded.
                                    Console.WriteLine("Current Size:" + localFileSize + writer.Length + " (" + (int)(progress * 100) + " %)");
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Downloaded: SBRW Game Files Package".ToUpper());
            }
        }
    }
}