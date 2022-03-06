using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Hash_Reader_LZMA
    {
        /// <summary>
        /// 
        /// </summary>
        public const int MaxWorkers = 3;
        /// <summary>
        /// 
        /// </summary>
        public const string HashFileName = "HashFile";
        /// <summary>
        /// 
        /// </summary>
        readonly Dictionary<string, Download_Hash_Reader_LZMA.HashTuple> _fileList;
        /// <summary>
        /// 
        /// </summary>
        readonly Queue<string> _queueHash;
        /// <summary>
        /// 
        /// </summary>
        private readonly static object _queueHashLock;
        /// <summary>
        /// 
        /// </summary>
        private static int _workerCount;
        /// <summary>
        /// 
        /// </summary>
        readonly bool _useCache = true;
        /// <summary>
        /// 
        /// </summary>
        readonly static Download_Hash_Reader_LZMA _instance;
        /// <summary>
        /// 
        /// </summary>
        internal static Download_Hash_Reader_LZMA Instance
        {
            get { return Download_Hash_Reader_LZMA._instance; }
        }
        /// <summary>
        /// 
        /// </summary>
        static Download_Hash_Reader_LZMA()
        {
            Download_Hash_Reader_LZMA._queueHashLock = new object();
            Download_Hash_Reader_LZMA._workerCount = 0;
            Download_Hash_Reader_LZMA._instance = new Download_Hash_Reader_LZMA();
        }
        /// <summary>
        /// 
        /// </summary>
        private Download_Hash_Reader_LZMA()
        {
            this._useCache = true;
            this._fileList = new Dictionary<string, Download_Hash_Reader_LZMA.HashTuple>();
            this._queueHash = new Queue<string>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs args)
        {
            while (true)
            {
                lock (Download_Hash_Reader_LZMA._queueHashLock)
                {
                    if (this._queueHash.Count == 0)
                    {
                        Download_Hash_Reader_LZMA._workerCount--;
                        break;
                    }
                }
                string str = null;
                lock (Download_Hash_Reader_LZMA._queueHashLock)
                {
                    str = this._queueHash.Dequeue();
                }
                string base64String = null;
                if (File.Exists(str))
                {
                    if (string.IsNullOrWhiteSpace(this._fileList[str].Old))
                    {
                        try
                        {
                            using (FileStream fileStream = File.OpenRead(str))
                            {
                                using (MD5 mD5 = MD5.Create())
                                {
                                    base64String = Convert.ToBase64String(mD5.ComputeHash(fileStream));
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        base64String = this._fileList[str].Old;
                    }
                }
                lock (this._fileList[str])
                {
                    this._fileList[str].Old = base64String;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_RunWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            lock (this._queueHash)
            {
                this._queueHash.Clear();
            }
            while (Download_Hash_Reader_LZMA._workerCount > 0)
            {
                Thread.Sleep(100);
            }
            this._fileList.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetHashOld(string fileName)
        {
            string empty = string.Empty;
            while (true)
            {
                lock (this._fileList[fileName])
                {
                    empty = this._fileList[fileName].Old;
                }
                if (empty != string.Empty)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            return empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool HashesMatch(string fileName)
        {
            bool @new;
            try
            {
                while (true)
                {
                    lock (this._fileList[fileName])
                    {
                        if (this._fileList[fileName].Old != string.Empty)
                        {
                            @new = this._fileList[fileName].New == this._fileList[fileName].Old;
                            break;
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return @new;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="patchPath"></param>
        /// <param name="hashFileNameSuffix"></param>
        /// <param name="maxWorkers"></param>
        public void Start(XmlDocument doc, string patchPath, string hashFileNameSuffix, int maxWorkers)
        {
            foreach (XmlNode xmlNodes in doc.SelectNodes("/index/fileinfo"))
            {
                string innerText = xmlNodes.SelectSingleNode("path").InnerText;
                string str = xmlNodes.SelectSingleNode("file").InnerText;
                if (!string.IsNullOrWhiteSpace(patchPath))
                {
                    int num = innerText.IndexOf("/");
                    innerText = (num < 0 ? patchPath : innerText.Replace(innerText.Substring(0, num), patchPath));
                }
                string str1 = string.Concat(innerText, "/", str);
                if (xmlNodes.SelectSingleNode("hash") != null)
                {
                    this._fileList.Add(str1, new Download_Hash_Reader_LZMA.HashTuple(string.Empty, xmlNodes.SelectSingleNode("hash").InnerText));
                    this._queueHash.Enqueue(str1);
                }
                else
                {
                    this._fileList.Add(str1, new Download_Hash_Reader_LZMA.HashTuple(null, null));
                }
            }
            if (this._useCache && File.Exists(string.Concat("HashFile", hashFileNameSuffix)))
            {
                FileStream fileStream = null;
                CryptoStream cryptoStream = null;
                StreamReader streamReader = null;
                try
                {
                    try
                    {
                        DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider()
                        {
                            Key = Encoding.ASCII.GetBytes("12345678"),
                            IV = Encoding.ASCII.GetBytes("12345678")
                        };
                        ICryptoTransform cryptoTransform = dESCryptoServiceProvider.CreateDecryptor();
                        fileStream = new FileStream(string.Concat("HashFile", hashFileNameSuffix), FileMode.Open);
                        cryptoStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Read);
                        streamReader = new StreamReader(cryptoStream);
                        string str2 = null;
                        while (true)
                        {
                            string str3 = streamReader.ReadLine();
                            str2 = str3;
                            if (str3 == null)
                            {
                                break;
                            }
                            string[] strArrays = str2.Split(new char[] { '\t' });
                            string str4 = strArrays[0];
                            if (this._fileList.ContainsKey(str4) && File.Exists(str4) && long.Parse(strArrays[2]) == (new FileInfo(str4)).LastWriteTime.Ticks)
                            {
                                this._fileList[str4].Old = strArrays[1];
                                this._fileList[str4].Ticks = long.Parse(strArrays[2]);
                            }
                        }
                    }
                    catch (CryptographicException cryptographicException1)
                    {
                        CryptographicException cryptographicException = cryptographicException1;
                        streamReader = null;
                        cryptoStream = null;
                        this._fileList.Clear();
                    }
                    catch (Exception)
                    {
                        this._fileList.Clear();
                    }
                }
                finally
                {
                    if (streamReader != null)
                    {
                        streamReader.Close();
                        streamReader.Dispose();
                    }
                    if (cryptoStream != null)
                    {
                        cryptoStream.Close();
                        cryptoStream.Dispose();
                    }
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                    File.Delete(string.Concat("HashFile", hashFileNameSuffix));
                }
            }
            Download_Hash_Reader_LZMA._workerCount = 0;
            while (Download_Hash_Reader_LZMA._workerCount < maxWorkers && this._queueHash.Count > 0)
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(this.BackgroundWorker_DoWork);
                backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerComplete);
                backgroundWorker.RunWorkerAsync();
                Download_Hash_Reader_LZMA._workerCount++;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashFileNameSuffix"></param>
        /// <param name="writeOldHashes"></param>
        public void WriteHashCache(string hashFileNameSuffix, bool writeOldHashes)
        {
            lock (this._fileList)
            {
                FileStream fileStream = null;
                CryptoStream cryptoStream = null;
                StreamWriter streamWriter = null;
                try
                {
                    try
                    {
                        fileStream = new FileStream(string.Concat("HashFile", hashFileNameSuffix), FileMode.Create);
                        DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
                        byte[] bytes = Encoding.ASCII.GetBytes("12345678");
                        byte[] numArray = bytes;
                        dESCryptoServiceProvider.IV = bytes;
                        dESCryptoServiceProvider.Key = numArray;
                        cryptoStream = new CryptoStream(fileStream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
                        streamWriter = new StreamWriter(cryptoStream);
                        foreach (string key in this._fileList.Keys)
                        {
                            string empty = string.Empty;
                            empty = (!writeOldHashes ? this._fileList[key].New : this._fileList[key].Old);
                            if (!File.Exists(key) || string.IsNullOrWhiteSpace(empty))
                            {
                                continue;
                            }
                            DateTime lastWriteTime = (new FileInfo(key)).LastWriteTime;
                            streamWriter.WriteLine(string.Format("{0}\t{1}\t{2}", key, empty, lastWriteTime.Ticks));
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                finally
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Close();
                        streamWriter.Dispose();
                    }
                    if (cryptoStream != null)
                    {
                        cryptoStream.Close();
                        cryptoStream.Dispose();
                    }
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private class HashTuple
        {
            /// <summary>
            /// 
            /// </summary>
            public string Old;
            /// <summary>
            /// 
            /// </summary>
            public string New;
            /// <summary>
            /// 
            /// </summary>
            public long Ticks;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="oldHash"></param>
            /// <param name="newHash"></param>
            /// <param name="ticks"></param>
            public HashTuple(string oldHash, string newHash, long ticks)
            {
                this.Old = oldHash;
                this.New = newHash;
                this.Ticks = ticks;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="oldHash"></param>
            /// <param name="newHash"></param>
            public HashTuple(string oldHash, string newHash) : this(oldHash, newHash, DateTime.Now.AddYears(1).Ticks)
            {
            }
        }
    }
}
