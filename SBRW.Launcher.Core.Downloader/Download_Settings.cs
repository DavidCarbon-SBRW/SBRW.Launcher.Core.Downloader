﻿using System;
using System.Diagnostics;
using System.IO;

namespace SBRW.Launcher.Core.Downloader
{
    /// <summary>
    /// 
    /// </summary>
    public static class Download_Settings
    {
        private static string Version_Cache { get; set; } = "0.0.2.4";
        private static bool Version_Check { get; set; }
        private static string Version 
        {
            get
            {
                if(!Version_Check)
                {
                    try
                    {
                        if (File.Exists("SBRW.Launcher.Core.Downloader.dll"))
                        {
#if !NETFRAMEWORK
#pragma warning disable CS8601 // Possible null reference assignment.
#endif
                            Version_Cache = FileVersionInfo.GetVersionInfo("SBRW.Launcher.Core.Downloader.dll").FileVersion;
#if !NETFRAMEWORK
#pragma warning restore CS8601 // Possible null reference assignment.
#endif
                        }
                    }
                    catch
                    {
                        /* Ignore Issue */
                    }

                    Version_Check = true;
                }
                
                if(string.IsNullOrWhiteSpace(Version_Cache))
                {
                    Version_Cache = "0.0.2.0";
                }

                return Version_Cache; 
            } 
        }
        /// <summary>
        /// 
        /// </summary>
        public static bool System_Unix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        internal static bool Alternative_WebCalls_Cache { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool Alternative_WebCalls() => Alternative_WebCalls_Cache;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Provided_Bool"></param>
        /// <returns></returns>
        public static bool Alternative_WebCalls(bool Provided_Bool)
        {
            return Alternative_WebCalls_Cache = Provided_Bool;
        }
        /// <summary>
        /// 
        /// </summary>
        internal static string Header = "SBRW.Launcher.Core.Downloader Version " + Version + " (+https://github.com/DavidCarbon-SBRW/SBRW.Launcher.Core.Downloader)";
        /// <summary>
        /// Global Boolen for Web Call Timeout before termining the connection
        /// </summary>
        public static bool Launcher_WebCall_Timeout_Enable { get; set; }
        /// <summary>
        /// Cached Internal Value
        /// </summary>
        internal static int Launcher_WebCall_Timeout_Cache { get; set; } = 30;
        /// <summary>
        /// Global Web Call Timeout before termining the connection
        /// </summary>
        /// <returns></returns>
        public static int Launcher_WebCall_Timeout() => Launcher_WebCall_Timeout_Cache;
        /// <summary>
        /// Global Web Call Timeout before termining the connection
        /// </summary>
        /// <param name="Provided_Seconds">Seconds in int</param>
        /// <returns></returns>
        public static int Launcher_WebCall_Timeout(int Provided_Seconds)
        {
            try
            {
                if (Provided_Seconds <= 0 || Provided_Seconds >= 180)
                {
                    return Launcher_WebCall_Timeout_Cache = 30;
                }
                else
                {
                    return Launcher_WebCall_Timeout_Cache = Provided_Seconds;
                }
            }
            catch (Exception)
            {
                return Launcher_WebCall_Timeout_Cache = 30;
            }
        }
    }
}
