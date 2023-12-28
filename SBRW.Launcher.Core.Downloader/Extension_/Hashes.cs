using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SBRW.Launcher.Core.Downloader.Extension_
{
    /// <summary>
    /// 
    /// </summary>
    internal class Hashes
    {
        /// <summary>
        /// String Hash based on Selection
        /// </summary>
        /// <param name="Hash_Mode">
        /// <code>
        /// 0 -> MD5
        /// 1 -> SHA-1
        /// 2 -> SHA-256
        /// 3 -> SHA-512
        /// </code>
        /// </param>
        /// <param name="Input_String">String Text Format</param>
        /// <returns>Choosen Hashed String</returns>
        public static string Hash_String(int Hash_Mode, string Input_String)
        {
            if (string.IsNullOrWhiteSpace(Input_String))
            {
                return string.Empty;
            }
            else
            {
                using HashAlgorithm Computer_Algorithm = Hash_Mode switch
                {
                    0 => MD5.Create(),
                    2 => SHA256.Create(),
                    3 => SHA512.Create(),
                    _ => SHA1.Create()
                };

                StringBuilder HashBuilder = new StringBuilder();
                foreach (byte Live_Byte in Computer_Algorithm.ComputeHash(Encoding.UTF8.GetBytes(Input_String)))
                {
                    HashBuilder.Append(Live_Byte.ToString("X2"));
                }

                return HashBuilder.ToString();
            }
        }
        /// <summary>
        /// File Hash in SHA-1
        /// </summary>
        /// <param name="File_Name">File Path in a String Format</param>
        /// <returns>SHA-1 Hash String</returns>
        public static string Hash_SHA(string File_Name)
        {
            if (!File.Exists(File_Name))
            {
                return string.Empty;
            }
            else
            {
                using (SHA1 SHA1_Generator = SHA1.Create())
                {
                    using (FileStream File_Stream = File.OpenRead(File_Name))
                    {
                        byte[] DataBase = SHA1_Generator.ComputeHash(File_Stream);
                        StringBuilder HashBuilder = new StringBuilder(DataBase.Length * 2);

                        for (int i = 0; i < DataBase.Length; i++)
                        {
                            HashBuilder.Append(DataBase[i].ToString("x2"));
                        }

                        return HashBuilder.ToString().ToUpperInvariant();
                    }
                }
            }
        }
    }
}
