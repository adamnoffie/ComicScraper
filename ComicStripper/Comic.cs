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
        /// Url to the PAGE with the latest edition of the strip,
        /// <example>e.g. http://comics.com/pearls_before_swine</example>
        /// </summary>
        [XmlIgnore]
        public string Url {
            get { return _url; }
            set {
                _url = value;
                ProcessComicUrl();
            }
        }

        private string _url;

        /// <summary>
        /// Regex string used to search the page for the actual img URL
        /// <example>
        /// </example>
        /// <![CDATA[<p class=\"feature_item\">.*?src='(?<url>.*?)'.*?alt='(?<title>.*?)'(?<alt>)]]>
        /// </summary>
        [XmlIgnore]
        public string SearchRegex { get; set; }

        /// <summary>
        /// Used for identifying the image as it is embedded in an email
        /// </summary>
        [XmlIgnore]
        public string ContentID { get { return Title.Replace(" ", "") + "_" + _contentIDGuid; } }
        private string _contentIDGuid;

        /// <summary>
        /// alt attribute on the stripped img tag
        /// </summary>
        [XmlIgnore]
        public string AltText { get; set; }

        /// <summary>
        /// title attribute on the stripped img tag
        /// </summary>
        [XmlIgnore]
        public string ToolTip { get; set; }

        [XmlAttribute]
        public string PreviousImgUrl { get; set; }

        [XmlAttribute]
        public int PreviousImgSize { get; set; }

        [XmlIgnore]
        public bool IsNewComic { get; set; }

        [XmlIgnore]
        public string StripImgFilePath { get; set; }

        public Comic()
        {            
            Title = Url = SearchRegex = PreviousImgUrl =
                string.Empty;
            PreviousImgSize = -1;
            IsNewComic = true;

            _contentIDGuid = Guid.NewGuid().ToString().Substring(0, 8);
        }

        private const string DatePlaceHolder = "{yyyy/mm/dd}";

        // handles any special indicators in comic url... currently just {{yyyy/mm/dd}}
        private void ProcessComicUrl()
        {
            if (_url.Contains(DatePlaceHolder))
            {
                _url = _url.Replace(DatePlaceHolder, DateTime.Now.ToString("yyyy/MM/dd"));
            }
        }

    }
}
