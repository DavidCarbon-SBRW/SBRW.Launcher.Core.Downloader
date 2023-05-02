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
                HashAlgorithm Computer_Algorithm;

                switch (Hash_Mode)
                {
                    case 0:
                        Computer_Algorithm = MD5.Create();
                        break;
                    case 2:
                        Computer_Algorithm = SHA256.Create();
                        break;
                    case 3:
                        Computer_Algorithm = SHA512.Create();
                        break;
                    default:
                        Computer_Algorithm = SHA1.Create();
                        break;
                }

                StringBuilder Complied_Hash = new StringBuilder();
                foreach (byte Live_Byte in Computer_Algorithm.ComputeHash(Encoding.UTF8.GetBytes(Input_String)))
                {
                    Complied_Hash.Append(Live_Byte.ToString("X2"));
                }

                return Complied_Hash.ToString();
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
                SHA1 sha1_Generator = SHA1.Create();
                byte[] retrived_Value = new byte[] { };

                using (var test = File.OpenRead(File_Name))
                {
                    retrived_Value = sha1_Generator.ComputeHash(test);
                }

                StringBuilder stringHashBuilder = new StringBuilder();
                for (int i = 0; i < retrived_Value.Length; i++)
                {
                    stringHashBuilder.Append(retrived_Value[i].ToString("x2"));
                }

                return stringHashBuilder.ToString().ToUpperInvariant();
            }
        }
    }
}
