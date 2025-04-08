using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComicScraper
{
    public static class Constants
    {
        public const string EmailTemplate = @"<font face='Arial,sans serif'><p><b>[[TITLE]]</b><br> 
<a href='[[LINK]]' target='_new'>
<img style='border:1;border-color:black;border-style:solid' src='cid:[[IMAGE]]'
title='[[ALTTEXT]]' alt='[[TITLE]]' ></a></p><br>[[ALTTEXT]]";

        public const string ComicsFile = "Comics.txt";
        public const string ComicsHistoryFile = "History.xml";
        public const string ComicsLogFile = "Log.txt";
        
        public const string ComicStripImgPath = "Comics\\";

        /// <summary>
        /// Format String with: 0 - filename. NOTE: need to add an extension
        /// </summary>
        public const string ComicStripImgFilePath = "Comics\\{0}";

        public struct Sections
        {
            public const string Regex = "regex";
            public const string Comics = "comics";
        }
    }
}
