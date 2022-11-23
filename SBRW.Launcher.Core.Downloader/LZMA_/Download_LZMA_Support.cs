namespace SBRW.Launcher.Core.Downloader.LZMA_
{
    /// <summary>
    /// 
    /// </summary>
    public class Download_LZMA_Support
    {
        /// <summary>
        /// 
        /// </summary>
        public static string Speech_Language { get; set; } = "en";
        /// <summary>
        /// Check System Language and Return Current Lang for Speech Files
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        public static string SpeechFiles(string Language = "")
        {
            string CurrentLang = (!string.IsNullOrWhiteSpace(Language)) ? Language.ToLower() : string.Empty;
            if (CurrentLang == "eng")
            {
                Speech_Language = "en";
            }
            else if (CurrentLang == "ger" || CurrentLang == "deu")
            {
                Speech_Language = "de";
            }
            else if (CurrentLang == "rus")
            {
                Speech_Language = "ru";
            }
            else if (CurrentLang == "spa")
            {
                Speech_Language = "es";
            }
            else
            {
                Speech_Language = "en";
            }

            return Speech_Language;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int SpeechFilesSize()
        {
            string CurrentLang = SpeechFiles();

            if (CurrentLang == "eng" || CurrentLang == "en")
            {
                return 141805935;
            }
            else if (CurrentLang == "ger" || CurrentLang == "deu" || CurrentLang == "de")
            {
                return 105948386;
            }
            else if (CurrentLang == "rus" || CurrentLang == "ru")
            {
                return 121367723;
            }
            else if (CurrentLang == "spa" || CurrentLang == "es")
            {
                return 101540466;
            }
            else
            {
                return 141805935;
            }
        }
    }
}
