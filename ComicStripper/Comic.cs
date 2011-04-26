using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ComicStripper
{
    [Serializable]
    public class Comic
    {
        /// <summary>
        /// Title of the Comic
        /// <example>Pearls Before Swine</example>
        /// </summary>
        [XmlAttribute]
        public string Title { get; set; }

        /// <summary>
        /// Url to the PAGE with the latest edition of the strip
        /// <example>http://comics.com/pearls_before_swine</example>
        /// </summary>
        [XmlIgnore]
        public string Url { get; set; }

        /// <summary>
        /// Regex string used to search the page for the actual img URL
        /// <example>
        /// </example>
        /// <![CDATA[<p class=\"feature_item\">.*?src='(?<url>.*?)'.*?alt='(?<title>.*?)'(?<alt>)]]>
        /// </summary>
        [XmlIgnore]
        public string SearchRegex { get; set; }

        [XmlAttribute]
        public string PreviousImgUrl { get; set; }

        [XmlAttribute]
        public int PreviousImgSize { get; set; }

        [XmlIgnore]
        public bool IsNewComic { get; set; }

        [XmlIgnore]
        public byte[] StripImg { get; set; }

        public Comic()
        {            
            Title = Url = SearchRegex = PreviousImgUrl =
                string.Empty;
            PreviousImgSize = -1;
            IsNewComic = false;
        }
    }
}
