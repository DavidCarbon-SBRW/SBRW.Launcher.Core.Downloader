using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SBRW.Library.Debugger
{
    public class File_Info
    {
        string File_Hash { get; set; } = string.Empty;
        string File_Path { get; set; } = string.Empty;
    }
    internal class ChecksumsConversion
    {
        public static void Convert()
        {

            String[] getFilesToCheck = { };

            if (File.Exists("checksums.dat"))
            {
                /* Read Local checksums.dat */
                getFilesToCheck = File.ReadAllLines("checksums.dat");
            }
            else
            {
                /* Fetch and Read Remote checksums.dat */
                Console.WriteLine("Downloading Checksums File");

                Uri URLCall = new Uri("https://g2-sbrw.davidcarbon.download/unpacked/checksums.dat");
                var Client = new WebClient
                {
                    Encoding = Encoding.UTF8
                };
                Client.Headers.Add("user-agent", "SBRW Launcher (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");

                bool ErrorFree = true;

                try
                {
                    getFilesToCheck = Client.DownloadString(URLCall).Split('\n');
                }
                finally
                {
                    Client?.Dispose();
                }

                if (ErrorFree)
                {
                    File.WriteAllLines("checksums.dat", getFilesToCheck);
                }
            }

            string Raw_Checksum_List = "[";

            for (var i = 0; i < getFilesToCheck.Length; i++)
            {
                var Raw_Line = getFilesToCheck[i].Split(' ');
                Raw_Checksum_List += "{ \"Hash\": \"" + Raw_Line[0] + "\", \"Path\": \"" + Raw_Line[1].Replace("\\", "\\\\") + "\"},";
            }

            Raw_Checksum_List += "]";

            File.WriteAllText("Files.json", Raw_Checksum_List);
        }
    }
}
