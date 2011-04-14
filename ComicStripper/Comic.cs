using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComicStripper
{
    public class Comic
    {
        /// <summary>
        /// Title of the Comic
        /// <example>Pearls Before Swine</example>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Url to the latest edition of the strip
        /// <example>http://comics.com/pearls_before_swine</example>
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Regex string used to search the page for the actual img URL
        /// <example>
        /// </example>
        /// <![CDATA[<p class=\"feature_item\">.*?src='(?<url>.*?)'.*?alt='(?<title>.*?)'(?<alt>)]]>
        /// </summary>
        public string SearchRegex { get; set; }
    }
}
