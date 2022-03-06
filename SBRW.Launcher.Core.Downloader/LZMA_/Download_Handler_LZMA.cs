using SBRW.Launcher.Core.Downloader.Web_;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Xml;

namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_Handler_LZMA
    {
        /// <summary>
        /// 
        /// </summary>
        public const int MaxWorkers = 3;
        /// <summary>
        /// 
        /// </summary>
        public const int MaxActiveChunks = 16;
        /// <summary>
        /// 
        /// </summary>
        private static int _workerCount;
        /// <summary>
        /// 
        /// </summary>
        private int _maxWorkers;
        /// <summary>
        /// 
        /// </summary>
        readonly Dictionary<string, Download_Handler_LZMA.DownloadItem> _downloadList;
        /// <summary>
        /// 
        /// </summary>
        readonly LinkedList<string> _downloadQueue;
        /// <summary>
        /// 
        /// </summary>
        readonly List<BackgroundWorker> _workers;
        /// <summary>
        /// 
        /// </summary>
        private int _freeChunks;
        /// <summary>
        /// 
        /// </summary>
        private object _freeChunksLock;
        /// <summary>
        /// 
        /// </summary>
        private bool _managerRunning;
        /// <summary>
        /// 
        /// </summary>
        public bool ManagerRunning
        {
            get { return this._managerRunning; }
        }
        /// <summary>
        /// 
        /// </summary>
        static Download_Handler_LZMA()
        {
            Download_Handler_LZMA._workerCount = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public Download_Handler_LZMA() : this(3, 16)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxWorkers"></param>
        /// <param name="maxActiveChunks"></param>
        public Download_Handler_LZMA(int maxWorkers, int maxActiveChunks)
        {
            this._maxWorkers = maxWorkers;
            this._freeChunks = maxActiveChunks;
            this._downloadList = new Dictionary<string, Download_Handler_LZMA.DownloadItem>();
            this._downloadQueue = new LinkedList<string>();
            this._workers = new List<BackgroundWorker>();
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
                            if (this._freeChunks <= 0)
                            {
                                Thread.Sleep(100);
                            }
                            else
                            {
                                lock (this._downloadQueue)
                                {
                                    if (this._downloadQueue.Count == 0)
                                    {
                                        lock (this._workers)
                                        {
                                            this._workers.Remove((BackgroundWorker)sender);
                                        }
                                        Download_Handler_LZMA._workerCount--;
                                        break;
                                    }
                                }
                                string value = null;
                                lock (this._downloadQueue)
                                {
                                    value = this._downloadQueue.Last.Value;
                                    this._downloadQueue.RemoveLast();
                                    lock (this._freeChunksLock)
                                    {
                                        this._freeChunks--;
                                    }
                                }
                                lock (this._downloadList[value])
                                {
                                    if (this._downloadList[value].Status != Download_Handler_LZMA.DownloadStatus.Canceled)
                                    {
                                        this._downloadList[value].Status = Download_Handler_LZMA.DownloadStatus.Downloading;
                                    }
                                }
                                while (webClient.IsBusy)
                                {
                                    Thread.Sleep(100);
                                }
                                webClient.DownloadDataAsync(new Uri(value), value);
                                Download_Handler_LZMA.DownloadStatus status = Download_Handler_LZMA.DownloadStatus.Downloading;
                                while (status == Download_Handler_LZMA.DownloadStatus.Downloading)
                                {
                                    status = this._downloadList[value].Status;
                                    if (status == Download_Handler_LZMA.DownloadStatus.Canceled)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(100);
                                }
                                if (status == Download_Handler_LZMA.DownloadStatus.Canceled)
                                {
                                    webClient.CancelAsync();
                                }
                                lock (this._workers)
                                {
                                    if (Download_Handler_LZMA._workerCount > this._maxWorkers || !this._managerRunning)
                                    {
                                        this._workers.Remove((BackgroundWorker)sender);
                                        Download_Handler_LZMA._workerCount--;
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
                            if (this._freeChunks <= 0)
                            {
                                Thread.Sleep(100);
                            }
                            else
                            {
                                lock (this._downloadQueue)
                                {
                                    if (this._downloadQueue.Count == 0)
                                    {
                                        lock (this._workers)
                                        {
                                            this._workers.Remove((BackgroundWorker)sender);
                                        }
                                        Download_Handler_LZMA._workerCount--;
                                        break;
                                    }
                                }
                                string value = null;
                                lock (this._downloadQueue)
                                {
                                    value = this._downloadQueue.Last.Value;
                                    this._downloadQueue.RemoveLast();
                                    lock (this._freeChunksLock)
                                    {
                                        this._freeChunks--;
                                    }
                                }
                                lock (this._downloadList[value])
                                {
                                    if (this._downloadList[value].Status != Download_Handler_LZMA.DownloadStatus.Canceled)
                                    {
                                        this._downloadList[value].Status = Download_Handler_LZMA.DownloadStatus.Downloading;
                                    }
                                }
                                while (webClient.IsBusy)
                                {
                                    Thread.Sleep(100);
                                }
                                webClient.DownloadDataAsync(new Uri(value), value);
                                Download_Handler_LZMA.DownloadStatus status = Download_Handler_LZMA.DownloadStatus.Downloading;
                                while (status == Download_Handler_LZMA.DownloadStatus.Downloading)
                                {
                                    status = this._downloadList[value].Status;
                                    if (status == Download_Handler_LZMA.DownloadStatus.Canceled)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(100);
                                }
                                if (status == Download_Handler_LZMA.DownloadStatus.Canceled)
                                {
                                    webClient.CancelAsync();
                                }
                                lock (this._workers)
                                {
                                    if (Download_Handler_LZMA._workerCount > this._maxWorkers || !this._managerRunning)
                                    {
                                        this._workers.Remove((BackgroundWorker)sender);
                                        Download_Handler_LZMA._workerCount--;
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
                lock (this._workers)
                {
                    this._workers.Remove((BackgroundWorker)sender);
                    Download_Handler_LZMA._workerCount--;
                }
            }
            finally
            {
                GC.Collect();
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
            lock (this._downloadQueue)
            {
                this._downloadQueue.Clear();
            }
            foreach (string key in this._downloadList.Keys)
            {
                lock (this._downloadList[key])
                {
                    if (this._downloadList[key].Data != null)
                    {
                        lock (this._freeChunksLock)
                        {
                            this._freeChunks++;
                        }
                    }
                    this._downloadList[key].Status = Download_Handler_LZMA.DownloadStatus.Canceled;
                    this._downloadList[key].Data = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void CancelDownload(string fileName)
        {
            lock (this._downloadQueue)
            {
                if (this._downloadQueue.Contains(fileName))
                {
                    this._downloadQueue.Remove(fileName);
                }
            }
            if (this._downloadList.ContainsKey(fileName))
            {
                lock (this._downloadList[fileName])
                {
                    if (this._downloadList[fileName].Data != null)
                    {
                        lock (this._freeChunksLock)
                        {
                            this._freeChunks++;
                        }
                    }
                    this._downloadList[fileName].Status = Download_Handler_LZMA.DownloadStatus.Canceled;
                    this._downloadList[fileName].Data = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.CancelAllDownloads();
            while (Download_Handler_LZMA._workerCount > 0)
            {
                Thread.Sleep(100);
            }
            lock (this._downloadList)
            {
                this._downloadList.Clear();
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
                    if (this._downloadList.ContainsKey(str))
                    {
                        lock (this._downloadList[str])
                        {
                            if (this._downloadList[str].Status == Download_Handler_LZMA.DownloadStatus.Canceled || this._maxWorkers <= 1)
                            {
                                this._downloadList[str].Data = null;
                                this._downloadList[str].Status = Download_Handler_LZMA.DownloadStatus.Canceled;
                            }
                            else
                            {
                                this._downloadList[str].Data = null;
                                this._downloadList[str].Status = Download_Handler_LZMA.DownloadStatus.Queued;
                                lock (this._downloadQueue)
                                {
                                    this._downloadQueue.AddLast(str);
                                }
                                lock (this._workers)
                                {
                                    this._maxWorkers--;
                                }
                            }
                        }
                    }
                }
                lock (this._freeChunksLock)
                {
                    this._freeChunks++;
                }
            }
            else
            {
                lock (this._downloadList[str])
                {
                    if (this._downloadList[str].Status != Download_Handler_LZMA.DownloadStatus.Downloaded)
                    {
                        this._downloadList[str].Data = new byte[(int)e.Result.Length];
                        Buffer.BlockCopy(e.Result, 0, this._downloadList[str].Data, 0, (int)e.Result.Length);
                        this._downloadList[str].Status = Download_Handler_LZMA.DownloadStatus.Downloaded;
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
            Download_Handler_LZMA.DownloadStatus status;
            byte[] data = null;
            this.ScheduleFile(fileName);
            lock (this._downloadList[fileName])
            {
                status = this._downloadList[fileName].Status;
            }
            while (status != Download_Handler_LZMA.DownloadStatus.Downloaded && status != Download_Handler_LZMA.DownloadStatus.Canceled)
            {
                Thread.Sleep(100);
                lock (this._downloadList[fileName])
                {
                    status = this._downloadList[fileName].Status;
                }
            }
            if (this._downloadList[fileName].Status == Download_Handler_LZMA.DownloadStatus.Downloaded)
            {
                lock (this._downloadList[fileName])
                {
                    data = this._downloadList[fileName].Data;
                    this._downloadList[fileName].Data = null;
                    lock (this._freeChunksLock)
                    {
                        this._freeChunks++;
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
        public Download_Handler_LZMA.DownloadStatus? GetStatus(string fileName)
        {
            if (!this._downloadList.ContainsKey(fileName))
            {
                return null;
            }
            return new Download_Handler_LZMA.DownloadStatus?(this._downloadList[fileName].Status);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="serverPath"></param>
        public void Initialize(XmlDocument doc, string serverPath)
        {
            this._freeChunksLock = new object();
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
                if (!this._downloadList.ContainsKey(str1))
                {
                    this._downloadList.Add(str1, new Download_Handler_LZMA.DownloadItem());
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void ScheduleFile(string fileName)
        {
            if (this._downloadList.ContainsKey(fileName))
            {
                Download_Handler_LZMA.DownloadStatus status = Download_Handler_LZMA.DownloadStatus.Queued;
                lock (this._downloadList[fileName])
                {
                    status = this._downloadList[fileName].Status;
                }
                if (status != Download_Handler_LZMA.DownloadStatus.Queued && status != Download_Handler_LZMA.DownloadStatus.Canceled)
                {
                    return;
                }
                lock (this._downloadQueue)
                {
                    if (this._downloadQueue.Contains(fileName) && this._downloadQueue.Last.Value != fileName)
                    {
                        this._downloadQueue.Remove(fileName);
                        this._downloadQueue.AddLast(fileName);
                    }
                    else if (!this._downloadQueue.Contains(fileName))
                    {
                        this._downloadQueue.AddLast(fileName);
                    }
                }
                lock (this._downloadList[fileName])
                {
                    this._downloadList[fileName].Status = Download_Handler_LZMA.DownloadStatus.Queued;
                }
            }
            else
            {
                this._downloadList.Add(fileName, new Download_Handler_LZMA.DownloadItem());
                lock (this._downloadQueue)
                {
                    this._downloadQueue.AddLast(fileName);
                }
            }
            if (this._managerRunning && Download_Handler_LZMA._workerCount < this._maxWorkers)
            {
                lock (this._workers)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(this.BackgroundWorker_DoWork);
                    backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerComplete);
                    backgroundWorker.RunWorkerAsync();
                    this._workers.Add(backgroundWorker);
                    Download_Handler_LZMA._workerCount++;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            this._managerRunning = true;
            lock (this._workers)
            {
                while (Download_Handler_LZMA._workerCount < this._maxWorkers)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(this.BackgroundWorker_DoWork);
                    backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerComplete);
                    backgroundWorker.RunWorkerAsync();
                    this._workers.Add(backgroundWorker);
                    Download_Handler_LZMA._workerCount++;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            this._managerRunning = false;
        }
        /// <summary>
        /// 
        /// </summary>
        private class DownloadItem
        {
            /// <summary>
            /// 
            /// </summary>
            public Download_Handler_LZMA.DownloadStatus Status;
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
                this.Status = Download_Handler_LZMA.DownloadStatus.Queued;
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
