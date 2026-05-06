using System.IO;

namespace ComicScraper
{
    public static class Constants
    {
        public const string EmailTemplate = @"<font face='Arial,sans serif'><p><b>[[TITLE]]</b><br> 
<a href='[[LINK]]' target='_new'>
<img style='border:1;border-color:black;border-style:solid' src='cid:[[IMAGE]]'
title='[[ALTTEXT]]' alt='[[TITLE]]' ></a></p><br>[[ALTTEXT]]";

        public const string ComicsHistoryFileName = "History.json";
        public const string ComicsLogFileName = "Log.txt";
        public const string ComicStripImgDirName = "Comics";

        public static string ComicsHistoryFile => Path.Combine(AppSettings.DataPath, ComicsHistoryFileName);
        public static string ComicsLogFile => Path.Combine(AppSettings.DataPath, ComicsLogFileName);
        public static string ComicStripImgPath => Path.Combine(AppSettings.DataPath, ComicStripImgDirName);

        /// <summary>
        /// Format String with: 0 - filename. NOTE: need to add an extension
        /// </summary>
        public static string ComicStripImgFilePath => Path.Combine(AppSettings.DataPath, ComicStripImgDirName, "{0}");
    }
}
