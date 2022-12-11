using SBRW.Launcher.Core.Downloader;
using System;
using System.IO;

namespace SBRW.Library.Debugger
{
    internal class Program
    {
        public static Download_Queue? Pack_SBRW_Downloader { get; set; }
        public static Download_Extract? Pack_SBRW_Unpacker { get; set; }
        private static int Pack_SBRW_Downloader_Time_Span { get; set; }
        public static bool Pack_SBRW_Downloader_Unpack_Lock { get; set; }

        private static string GameFolderPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles"); } }
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

                Pack_SBRW_Downloader = new Download_Queue();
                /* @DavidCarbon or @Zacam (Translation Strings Required) */
                Pack_SBRW_Downloader.Live_Progress += (x, D_Live_Events) =>
                {
                    if (!Pack_SBRW_Downloader.Cancel)
                    {
                        Console.WriteLine("(" + D_Live_Events.Download_Percentage + "%)");
                    }
                };
                Pack_SBRW_Downloader.Complete += (x, D_Live_Events) =>
                {
                    if (D_Live_Events.Complete && x != null)
                    {
                        Console.WriteLine("Downloaded: SBRW Game Files Package".ToUpper());
                    }
                };
                /* Main Note: Current Revision File Size (in long) is: 3862102244 */
                Pack_SBRW_Downloader.Download(Launcher_CDN + "/GameFiles.sbrwpack", GameFolderPath, string.Empty, 3862102244,
                    Launcher_CDN, "GameFiles.sbrwpack");
            }
        }
    }
}