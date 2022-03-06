using SBRW.Launcher.Core.Downloader.Web_;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Xml;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    internal class Download_LZMA_Data_Manager
    {
        /// <summary>
        /// 
        /// </summary>
        public int MaxWorkers { get { return 3; } }
        /// <summary>
        /// 
        /// </summary>
        public int MaxActiveChunks { get { return 16; } }
        private static int Worker_Count { get; set; }
        private int Workers_Max { get; set; }
        private Dictionary<string, DownloadItem> Download_List { get; set; }
        private LinkedList<string> Download_Queue { get; set; }
        private List<BackgroundWorker> Workers_Live { get; set; }
        private int Free_Chunks { get; set; }
        private object Free_ChunksLock { get; set; }
        private bool Manager_Running { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ManagerRunning
        {
            get { return this.Manager_Running; }
        }

        static Download_LZMA_Data_Manager()
        {
            Worker_Count = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public Download_LZMA_Data_Manager() : this(3, 16)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxWorkers"></param>
        /// <param name="maxActiveChunks"></param>
        public Download_LZMA_Data_Manager(int maxWorkers, int maxActiveChunks)
        {
            this.Workers_Max = maxWorkers;
            this.Free_Chunks = maxActiveChunks;
            this.Download_List = new Dictionary<string, Download_LZMA_Data_Manager.DownloadItem>();
            this.Download_Queue = new LinkedList<string>();
            this.Workers_Live = new List<BackgroundWorker>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                if (Download_Settings.Alternative_WebCalls)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add("user-agent", Download_Settings.Header_LZMA);
                        webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(this.DownloadManager_DownloadDataCompleted);
                        webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                        while (true)
                        {
                            if (this.Free_Chunks <= 0)
                            {
                                Thread.Sleep(100);
                            }
                            else
                            {
                                lock (this.Download_Queue)
                                {
                                    if (this.Download_Queue.Count == 0)
                                    {
                                        lock (this.Workers_Live)
                                        {
                                            this.Workers_Live.Remove((BackgroundWorker)sender);
                                        }
                                        Download_LZMA_Data_Manager.Worker_Count--;
                                        break;
                                    }
                                }
                                string value = string.Empty;
                                lock (this.Download_Queue)
                                {
                                    value = this.Download_Queue.Last.Value;
                                    this.Download_Queue.RemoveLast();
                                    lock (this.Free_ChunksLock)
                                    {
                                        this.Free_Chunks--;
                                    }
                                }
                                lock (this.Download_List[value])
                                {
                                    if (this.Download_List[value].Status != DownloadStatus.Canceled)
                                    {
                                        this.Download_List[value].Status = DownloadStatus.Downloading;
                                    }
                                }
                                while (webClient.IsBusy)
                                {
                                    Thread.Sleep(100);
                                }
                                webClient.DownloadDataAsync(new Uri(value), value);
                                DownloadStatus status = DownloadStatus.Downloading;
                                while (status == DownloadStatus.Downloading)
                                {
                                    status = this.Download_List[value].Status;
                                    if (status == DownloadStatus.Canceled)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(100);
                                }
                                if (status == DownloadStatus.Canceled)
                                {
                                    webClient.CancelAsync();
                                }
                                lock (this.Workers_Live)
                                {
                                    if (Worker_Count > this.Workers_Max || !this.Manager_Running)
                                    {
                                        this.Workers_Live.Remove((BackgroundWorker)sender);
                                        Worker_Count--;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (WebClientWithTimeout webClient = new WebClientWithTimeout())
                    {
                        webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(this.DownloadManager_DownloadDataCompleted);
                        webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                        while (true)
                        {
                            if (this.Free_Chunks <= 0)
                            {
                                Thread.Sleep(100);
                            }
                            else
                            {
                                lock (this.Download_Queue)
                                {
                                    if (this.Download_Queue.Count == 0)
                                    {
                                        lock (this.Workers_Live)
                                        {
                                            this.Workers_Live.Remove((BackgroundWorker)sender);
                                        }
                                        Worker_Count--;
                                        break;
                                    }
                                }
                                string value = string.Empty;
                                lock (this.Download_Queue)
                                {
                                    value = this.Download_Queue.Last.Value;
                                    this.Download_Queue.RemoveLast();
                                    lock (this.Free_ChunksLock)
                                    {
                                        this.Free_Chunks--;
                                    }
                                }
                                lock (this.Download_List[value])
                                {
                                    if (this.Download_List[value].Status != DownloadStatus.Canceled)
                                    {
                                        this.Download_List[value].Status = DownloadStatus.Downloading;
                                    }
                                }
                                while (webClient.IsBusy)
                                {
                                    Thread.Sleep(100);
                                }
                                webClient.DownloadDataAsync(new Uri(value), value);
                                DownloadStatus status = DownloadStatus.Downloading;
                                while (status == DownloadStatus.Downloading)
                                {
                                    status = this.Download_List[value].Status;
                                    if (status == DownloadStatus.Canceled)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(100);
                                }
                                if (status == DownloadStatus.Canceled)
                                {
                                    webClient.CancelAsync();
                                }
                                lock (this.Workers_Live)
                                {
                                    if (Worker_Count > this.Workers_Max || !this.Manager_Running)
                                    {
                                        this.Workers_Live.Remove((BackgroundWorker)sender);
                                        Worker_Count--;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                lock (this.Workers_Live)
                {
                    this.Workers_Live.Remove((BackgroundWorker)sender);
                    Worker_Count--;
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
        public void CancelAllDownloads()
        {
            this.Stop();
            lock (this.Download_Queue)
            {
                this.Download_Queue.Clear();
            }
            foreach (string key in this.Download_List.Keys)
            {
                lock (this.Download_List[key])
                {
                    if (this.Download_List[key].Data != null)
                    {
                        lock (this.Free_ChunksLock)
                        {
                            this.Free_Chunks++;
                        }
                    }
                    this.Download_List[key].Status = DownloadStatus.Canceled;
                    this.Download_List[key].Data = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void CancelDownload(string fileName)
        {
            lock (this.Download_Queue)
            {
                if (this.Download_Queue.Contains(fileName))
                {
                    this.Download_Queue.Remove(fileName);
                }
            }
            if (this.Download_List.ContainsKey(fileName))
            {
                lock (this.Download_List[fileName])
                {
                    if (this.Download_List[fileName].Data != null)
                    {
                        lock (this.Free_ChunksLock)
                        {
                            this.Free_Chunks++;
                        }
                    }
                    this.Download_List[fileName].Status = DownloadStatus.Canceled;
                    this.Download_List[fileName].Data = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.CancelAllDownloads();
            while (Worker_Count > 0)
            {
                Thread.Sleep(100);
            }
            lock (this.Download_List)
            {
                this.Download_List.Clear();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadManager_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            string str = e.UserState.ToString();
            if (e.Cancelled || e.Error != null)
            {
                if (e.Error != null)
                {
                    if (this.Download_List.ContainsKey(str))
                    {
                        lock (this.Download_List[str])
                        {
                            if (this.Download_List[str].Status == DownloadStatus.Canceled || this.Workers_Max <= 1)
                            {
                                this.Download_List[str].Data = null;
                                this.Download_List[str].Status = DownloadStatus.Canceled;
                            }
                            else
                            {
                                this.Download_List[str].Data = null;
                                this.Download_List[str].Status = DownloadStatus.Queued;
                                lock (this.Download_Queue)
                                {
                                    this.Download_Queue.AddLast(str);
                                }
                                lock (this.Workers_Live)
                                {
                                    this.Workers_Max--;
                                }
                            }
                        }
                    }
                }
                lock (this.Free_ChunksLock)
                {
                    this.Free_Chunks++;
                }
            }
            else
            {
                lock (this.Download_List[str])
                {
                    if (this.Download_List[str].Status != DownloadStatus.Downloaded)
                    {
                        this.Download_List[str].Data = new byte[(int)e.Result.Length];
                        Buffer.BlockCopy(e.Result, 0, this.Download_List[str].Data, 0, (int)e.Result.Length);
                        this.Download_List[str].Status = DownloadStatus.Downloaded;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] GetFile(string fileName)
        {
            DownloadStatus status;
            byte[] data = null;
            this.ScheduleFile(fileName);
            lock (this.Download_List[fileName])
            {
                status = this.Download_List[fileName].Status;
            }
            while (status != DownloadStatus.Downloaded && status != DownloadStatus.Canceled)
            {
                Thread.Sleep(100);
                lock (this.Download_List[fileName])
                {
                    status = this.Download_List[fileName].Status;
                }
            }
            if (this.Download_List[fileName].Status == DownloadStatus.Downloaded)
            {
                lock (this.Download_List[fileName])
                {
                    data = this.Download_List[fileName].Data;
                    this.Download_List[fileName].Data = null;
                    lock (this.Free_ChunksLock)
                    {
                        this.Free_Chunks++;
                    }
                }
            }
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public DownloadStatus? GetStatus(string fileName)
        {
            if (!this.Download_List.ContainsKey(fileName))
            {
                return null;
            }
            return new Download_LZMA_Data_Manager.DownloadStatus?(this.Download_List[fileName].Status);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="serverPath"></param>
        public void Initialize(XmlDocument doc, string serverPath)
        {
            this.Free_ChunksLock = new object();
            int num = 0;
            foreach (XmlNode xmlNodes in doc.SelectNodes("/index/fileinfo"))
            {
                string innerText = xmlNodes.SelectSingleNode("path").InnerText;
                string str = xmlNodes.SelectSingleNode("file").InnerText;
                if (xmlNodes.SelectSingleNode("section") == null)
                {
                    continue;
                }
                num = int.Parse(xmlNodes.SelectSingleNode("section").InnerText);
            }
            for (int i = 1; i <= num; i++)
            {
                string str1 = string.Format("{0}/section{1}.dat", serverPath, i);
                if (!this.Download_List.ContainsKey(str1))
                {
                    this.Download_List.Add(str1, new DownloadItem());
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void ScheduleFile(string fileName)
        {
            if (this.Download_List.ContainsKey(fileName))
            {
                DownloadStatus status = DownloadStatus.Queued;
                lock (this.Download_List[fileName])
                {
                    status = this.Download_List[fileName].Status;
                }
                if (status != DownloadStatus.Queued && status != DownloadStatus.Canceled)
                {
                    return;
                }
                lock (this.Download_Queue)
                {
                    if (this.Download_Queue.Contains(fileName) && this.Download_Queue.Last.Value != fileName)
                    {
                        this.Download_Queue.Remove(fileName);
                        this.Download_Queue.AddLast(fileName);
                    }
                    else if (!this.Download_Queue.Contains(fileName))
                    {
                        this.Download_Queue.AddLast(fileName);
                    }
                }
                lock (this.Download_List[fileName])
                {
                    this.Download_List[fileName].Status = DownloadStatus.Queued;
                }
            }
            else
            {
                this.Download_List.Add(fileName, new DownloadItem());
                lock (this.Download_Queue)
                {
                    this.Download_Queue.AddLast(fileName);
                }
            }
            if (this.Manager_Running && Worker_Count < this.Workers_Max)
            {
                lock (this.Workers_Live)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(this.BackgroundWorker_DoWork);
                    backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerComplete);
                    backgroundWorker.RunWorkerAsync();
                    this.Workers_Live.Add(backgroundWorker);
                    Worker_Count++;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            this.Manager_Running = true;
            lock (this.Workers_Live)
            {
                while (Worker_Count < this.Workers_Max)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(this.BackgroundWorker_DoWork);
                    backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerComplete);
                    backgroundWorker.RunWorkerAsync();
                    this.Workers_Live.Add(backgroundWorker);
                    Worker_Count++;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            this.Manager_Running = false;
        }
        /// <summary>
        /// 
        /// </summary>
        private class DownloadItem
        {
            /// <summary>
            /// 
            /// </summary>
            public DownloadStatus Status;
            /// <summary>
            /// 
            /// </summary>
            private byte[] _data;
            /// <summary>
            /// 
            /// </summary>
            public byte[] Data
            {
                get { return this._data; }
                set { this._data = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            public DownloadItem()
            {
                this.Status = DownloadStatus.Queued;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public enum DownloadStatus
        {
            Queued,
            Downloading,
            Downloaded,
            Canceled
        }
    }
}
