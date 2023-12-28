using System.IO;
using System.IO.Compression;

namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// A Collection of Assembly Patches
    /// </summary>
    /// <remarks>Aimed to backport functions for older .NET Targets</remarks>
    public class Assembly_Patches
    {
        /// <summary>
        /// Opens a zip archive for reading at the specified path.
        /// </summary>
        /// <param name="File_Path">
        /// The path to the archive to open, specified as a relative or absolute path.
        /// A relative path is interpreted as relative to the current working directory.
        /// </param>
        /// <remarks><b><i>ZipFile.OpenRead</i></b> does not exist in .NET Framework 4.6</remarks>
        /// <returns>The opened zip archive.</returns>
        public static ZipArchive OpenRead(string File_Path)
        {
            /* Reference https://stackoverflow.com/questions/44318777/system-missingmethodexception-when-trying-to-read-zipfile-from-ziparchive-c-shar */
            /* Error Log from Launcher: https://web.archive.org/web/20231228082331/https://cdn.discordapp.com/attachments/1181133620145041448/1181134366278176779/Launcher.log */
            return new ZipArchive(File.OpenRead(File_Path), ZipArchiveMode.Read);
        }
    }
}