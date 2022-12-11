using System;
using System.Diagnostics;
using System.IO;

namespace SBRW.Library.Debugger
{
    public class File_and_Folder_Extention
    {
        public static bool GetFolderExclusion(string Folder_Name, string[] Strings_Or_Folders)
        {
            if (string.IsNullOrWhiteSpace(Folder_Name))
            {
                return true;
            }
            else
            {
                bool Final_Results = false;

                foreach (string Folder_List_Name in Strings_Or_Folders)
                {
                    if (Folder_Name.StartsWith(Folder_List_Name))
                    {
                        Final_Results = true;
                    }
                }

                return Final_Results;
            }
        }

        public static long GetDirectorySize(System.IO.DirectoryInfo Directory_Info, bool Recursive = true)
        {

            long Start_Directory_Size = default;

            try
            {
                if (Directory_Info == null || !Directory_Info.Exists)
                {
                    /* Return 0 while Directory does not exist. */
                    return Start_Directory_Size;
                }
                else
                {
                    /* Add size of files in the Current Directory to main size. */
                    foreach (System.IO.FileInfo File_Info in Directory_Info.GetFiles())
                    {
                        System.Threading.Interlocked.Add(ref Start_Directory_Size, File_Info.Length);
                    }

                    /* Loop on Sub Direcotries in the Current Directory and Calculate it's files size. */
                    if (Recursive)
                    {
                        System.Threading.Tasks.Parallel.ForEach(Directory_Info.GetDirectories(), (Sub_Directory) =>
                        System.Threading.Interlocked.Add(ref Start_Directory_Size, GetDirectorySize(Sub_Directory, Recursive)));
                    }
                }
            }
            catch (System.Exception Error)
            {
                Start_Directory_Size = -1;
            }

            /* Return full Size of this Directory. */
            return Start_Directory_Size;
        }

        public static long GetDirectorySize_GameFiles(System.IO.DirectoryInfo Directory_Info, bool Recursive = true)
        {

            long Start_Directory_Size = default;

            try
            {
                if (Directory_Info == null || !Directory_Info.Exists)
                {
                    /* Return 0 while Directory does not exist. */
                    return Start_Directory_Size;
                }
                else
                {
                    /* Add size of files in the Current Directory to main size. */
                    foreach (System.IO.FileInfo File_Info in Directory_Info.GetFiles())
                    {
                        System.Threading.Interlocked.Add(ref Start_Directory_Size, File_Info.Length);
                    }

                    /* Loop on Sub Direcotries in the Current Directory and Calculate it's files size. */
                    if (Recursive)
                    {
                        System.Threading.Tasks.Parallel.ForEach(Directory_Info.GetDirectories(), (Sub_Directory) =>
                        {
                            if (Sub_Directory != null)
                            {
                                if (!GetFolderExclusion(Sub_Directory.Name, new string[]
                                {
                                ".",
                                "scripts",
                                "MODS"
                                }))
                                {
                                    System.Threading.Interlocked.Add(ref Start_Directory_Size, GetDirectorySize_GameFiles(Sub_Directory, Recursive));
                                }
                            }
                        });
                    }
                }
            }
            catch (System.Exception Error)
            {
                Start_Directory_Size = -1;
            }

            /* Return full Size of this Directory. */
            return Start_Directory_Size;
        }
    }
    /// <summary>
    /// Provides access to information on a drive.
    /// </summary>
    public class Format_System_Storage
    {
        /// <summary>
        /// The amount of available free space on a drive, in bytes.
        /// </summary>
        /// <returns>The amount of free space available on the drive, in bytes.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public long AvailableFreeSpace { get; set; }
        /// <summary>
        /// The amount of available free space on a drive.
        /// </summary>
        /// <returns>The amount of free space available on the drive.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public string AvailableFreeSpace_Linux { get; set; }
        /// <summary>
        /// The name of the file system, such as NTFS or FAT32.
        /// </summary>
        /// <returns>The name of the file system on the specified drive.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public string DriveFormat { get; set; }
        /// <summary>
        /// The drive type, such as CD-ROM, removable, network, or fixed.
        /// </summary>
        /// <returns>One of the enumeration values that specifies a drive type.</returns>
        public DriveType DriveType { get; set; }
        /// <summary>
        /// A value that indicates whether a drive is ready.
        /// </summary>
        /// <returns>true if the drive is ready; false if the drive is not ready.</returns>
        public bool IsReady { get; set; }
        /// <summary>
        /// The name of a drive, such as C:\.
        /// </summary>
        /// <returns>The name of the drive.</returns>
        public string Name { get; set; }
        /// <summary>
        /// The root directory of a drive.
        /// </summary>
        /// <returns>An object that contains the root directory of the drive.</returns>
        public DirectoryInfo RootDirectory { get; set; }
        /// <summary>
        /// The total amount of free space available on a drive, in bytes.
        /// </summary>
        /// <returns>The total free space available on a drive, in bytes.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="DriveNotFoundException">The drive is not mapped or does not exist.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public long TotalFreeSpace { get; set; }
        /// <summary>
        /// The total size of storage space on a drive, in bytes.
        /// </summary>
        /// <returns>The total size of the drive, in bytes.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="DriveNotFoundException">The drive is not mapped or does not exist.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public long TotalSize { get; set; }
        /// <summary>
        /// The total size of storage space on a drive.
        /// </summary>
        /// <returns>The total size of the drive.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="DriveNotFoundException">The drive is not mapped or does not exist.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public string TotalSize_Linux { get; set; }
        /// <summary>
        /// The volume label of a drive.
        /// </summary>
        /// <returns>The volume label.</returns>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        /// <exception cref="DriveNotFoundException">The drive is not mapped or does not exist.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="UnauthorizedAccessException">The volume label is being set on a network or CD-ROM drive. -or- 
        /// Access to the drive information is denied.</exception>
        public string VolumeLabel { get; set; }
        /// <summary>
        /// The total size of storage space being used on a drive, in bytes.
        /// </summary>
        /// <returns>The total size of the drive being used, in bytes.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="DriveNotFoundException">The drive is not mapped or does not exist.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public long TotalSizeUsed { get; set; }
        /// <summary>
        /// The total size of storage space being used on a drive.
        /// </summary>
        /// <returns>The total size of the drive being used.</returns>
        /// <exception cref="UnauthorizedAccessException">Access to the drive information is denied.</exception>
        /// <exception cref="DriveNotFoundException">The drive is not mapped or does not exist.</exception>
        /// <exception cref="IOException">An I/O error occurred (for example, a disk error or a drive was not ready).</exception>
        public string TotalSizeUsed_Linux { get; set; }
        /// <summary>
        /// Percentage of the Drive being Used
        /// </summary>
        /// <remarks>Linux Only</remarks>
        public int Percentage_Of_Drive_Used { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class System_Storage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Process StartProcess_Linux(string cmd, string args)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(cmd, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            return Process.Start(processStartInfo);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string ReadProcessOutput_Linux(string cmd, string args)
        {
            try
            {
                using Process process = StartProcess_Linux(cmd, args);
                using StreamReader streamReader = process.StandardOutput;
                process.WaitForExit();

                return streamReader.ReadToEnd().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Provides access to information on a drive.
        /// </summary>
        /// <param name="Folder_File_Path">A valid drive path or drive letter. 
        /// This can be either uppercase or lowercase, 'a' to 'z'. A null value is not valid.</param>
        /// <param name="Human_Values">Retrives Human-Readable Values from Bash</param>
        /// <param name="Unix_Platforms">Returns Unix Related Information</param>
        /// <returns>Converted Drive Information</returns>
        public static Format_System_Storage Drive_Full_Info(string Folder_File_Path, bool Unix_Platforms = false, bool Human_Values = false)
        {
            Format_System_Storage Current_Drive = default;

            if (Unix_Platforms)
            {
                /* Example */
                /* df -h / "Filesystem      Size  Used Avail Use% Mounted on  /dev/sda3        39G   30G  6.9G  82% /" */
                /* df / "Filesystem     1K-blocks     Used Available Use% Mounted on  /dev/sda3       40502528 31190956   7224456  82% /" */
                string Konsole = ReadProcessOutput_Linux(Human_Values ? "df -h " : "df ",
                    "'" + Folder_File_Path + "'".Replace("\"", "\\\""));
                string[] Konsole_Split = Konsole.Replace(' ', ',').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int.TryParse(Konsole_Split[12].Replace("%", string.Empty), out int Final_Value);

                if (Human_Values)
                {
                    long.TryParse(Konsole_Split[9], out long Long_Value_Total_Space);
                    long.TryParse(Konsole_Split[11], out long Long_Value_Available);

                    Current_Drive = new Format_System_Storage()
                    {
                        DriveFormat = "Unknown",
                        TotalSize = Long_Value_Total_Space * 1000L,
                        AvailableFreeSpace = Long_Value_Available * 1000L,
                        Name = Konsole_Split[13],
                        VolumeLabel = Konsole_Split[8],
                        RootDirectory = new DirectoryInfo(Folder_File_Path),
                        IsReady = true,
                        TotalFreeSpace = Long_Value_Total_Space * 1000L,
                        DriveType = DriveType.Unknown,
                        Percentage_Of_Drive_Used = Final_Value
                    };
                }
                else
                {
                    Current_Drive = new Format_System_Storage()
                    {
                        DriveFormat = "Unknown",
                        TotalSize_Linux = Konsole_Split[9],
                        AvailableFreeSpace_Linux = Konsole_Split[11],
                        Name = Konsole_Split[13],
                        VolumeLabel = Konsole_Split[8],
                        RootDirectory = new DirectoryInfo(Folder_File_Path),
                        IsReady = true,
                        TotalSizeUsed_Linux = Konsole_Split[10],
                        DriveType = DriveType.Unknown,
                        Percentage_Of_Drive_Used = Final_Value
                    };
                }
            }
            else
            {
                foreach (DriveInfo Next_Drive_In_List in DriveInfo.GetDrives())
                {
                    if (Next_Drive_In_List.Name == Path.GetPathRoot(Folder_File_Path))
                    {
                        Current_Drive = new Format_System_Storage()
                        {
                            DriveFormat = Next_Drive_In_List.DriveFormat,
                            AvailableFreeSpace = Next_Drive_In_List.AvailableFreeSpace,
                            DriveType = Next_Drive_In_List.DriveType,
                            TotalFreeSpace = Next_Drive_In_List.TotalFreeSpace,
                            TotalSize = Next_Drive_In_List.TotalSize,
                            IsReady = Next_Drive_In_List.IsReady,
                            Name = Next_Drive_In_List.Name,
                            RootDirectory = Next_Drive_In_List.RootDirectory,
                            VolumeLabel = Next_Drive_In_List.VolumeLabel
                        };

                        break;
                    }
                }
            }

            return Current_Drive;
        }
    }
}
