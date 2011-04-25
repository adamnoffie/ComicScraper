using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComicStripper
{
    public static class Constants
    {
        public const string EmailTemplate = @"<font face='Arial,sans serif'><p><b>[[TITLE]]</b><br> 
<a href='[[LINK]]' target='_new'>
<img style='border:1;border-color:black;border-style:solid' src='cid:[[IMAGE]]'
title='[[ALTTEXT]]' alt='[[TITLE]]' ></a></p><br>[[ALTTEXT]]";

        public const string ComicsFile = "Comics.txt";
        public const string ComicsHistoryFile = "History.xml";

        public struct Sections
        {
            public const string Regex = "regex";
            public const string Comics = "comics";
        }
    }
}
