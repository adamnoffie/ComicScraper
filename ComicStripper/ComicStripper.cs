using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ComicStripper.Properties;
using System.Threading;
using Util;

namespace ComicStripper
{
    class ComicStripper
    {
        string _emailBody = "";
        string _configFilePath = Path.Combine(Environment.CurrentDirectory, Constants.ComicsFile);
        string _historyFilePath = Path.Combine(Environment.CurrentDirectory, Constants.ComicsHistoryFile);
        
        List<ComicStripRegex> _regexes = new List<ComicStripRegex>();
        List<Comic> _comics = new List<Comic>();

        /// <summary>
        /// Run the ComicStripper
        /// </summary>
        public void Run()
        {
            if (!ReadConfigFile())
                return;

            ReadHistoryFile();

            foreach (Comic c in _comics)
            {
                RipComic(c);
                Thread.Sleep(5 * 1000);
            }
        }

        // see if we have a history file, with sizes and md5 checksums for previous comic fetch
        private void ReadHistoryFile()
        {
            if (File.Exists(_historyFilePath))
            {
                // read in the info about comics in the history XML file
                List<Comic> histories = XmlEx.FromXmlFile<List<Comic>>(_historyFilePath);
                foreach (Comic ch in histories)
                {
                    Comic c = _comics.Where(x => x.Title == ch.Title).FirstOrDefault();
                    if (c != null)
                    {
                        // update the comics list with the info gleaned from the history log
                        c.PreviousImgUrl = ch.PreviousImgUrl;
                        c.PreviousImgSize = ch.PreviousImgSize;
                    }
                }
            }
        }

        // rip a comic from the site
        private void RipComic(Comic c)
        {
            Logger.WriteLine("== Stripping {0} == {1} ==", c.Title, c.Url);

            // get the page that has the comic
            string pageHtml = FetchUrlAsString(c.Url);

            // get the comic image url
            var r = new Regex(c.SearchRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match = r.Match(pageHtml);

            if (match.Success && match.Groups["url"] != null && match.Groups["url"].Success)
            {
                string url = match.Groups["url"].Value;
                // this will handle the comic strip URL being absolute OR relative, and give you an absolute in the end
                Uri comicUri = new Uri(new Uri(c.Url), new Uri(url, UriKind.RelativeOrAbsolute));                
                string title = match.Groups["title"].Value;
                string alt = match.Groups["alt"].Value;

                Logger.WriteLine("url: {0}, title: {1}, alt: {2}", comicUri, title, alt);

                // quickly get (using HEAD) the strip image's size, then see if it is different comic
                // NOTE: this assumes different size is different comic, same size is same comic
                // it is highly unlikely that two strips in a row, by same author, would be exact same size in bytes
                int comicSize = (int)FetchUrlContentSize(comicUri, c.Url);
                if (comicSize != c.PreviousImgSize) // different size, do the comic
                {
                    c.PreviousImgUrl = comicUri.ToString();
                    HttpWebRequest req = WebRequest.Create(comicUri) as HttpWebRequest;
                    req.Method = "GET";
                    req.UserAgent = Settings.Default.UserAgent;
                    using (WebResponse resp = req.GetResponse())
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        // TODO:
                        //      Write the comic image to disk, and store the path in Comic object
                        //          - thus, refactor code just above this to    
                        //              FetchUrlToFile(string filePath)
                        //      Later, use: http://www.systemnetmail.com/faq/4.4.aspx to embed images in email

                    }
                }
                else // same size, same comic as last run, don't need to fetch
                {
                    Logger.WriteLine("Same size as Previous Fetch! Skipping.");
                    c.IsNewComic = false;
                }
                

                
            }
            else // regex matched nothing, or didn't find the "url" group!!!
            {
                Logger.WriteLine("oops!");

                
            }

            
        }

        // do an Http GET request of a url, and return the results as a string (good for HTML pages)
        private string FetchUrlAsString(string url)
        {
            // TODO: try catch, return null or string.Emtpy on error

            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "GET";
            req.UserAgent = Settings.Default.UserAgent;
            using (WebResponse resp = req.GetResponse())
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        // do an Http HEAD request to get the size of an object without fetching it
        private long FetchUrlContentSize(Uri comicUri, string referer)
        {
            HttpWebRequest req = WebRequest.Create(comicUri) as HttpWebRequest;
            req.Method = "HEAD";
            req.UserAgent = Settings.Default.UserAgent;
            req.Referer = referer;
            using (WebResponse resp = req.GetResponse())
            {
                return resp.ContentLength;
            }
        }

        // Read the Comics.txt configuration file to get regexes and comics to strip
        private bool ReadConfigFile()
        {
            if (!File.Exists(_configFilePath))
            {
                Logger.WriteLine("No Comic Config File (Comics.txt)! Exiting.");
                return false;
            }

            string currentSection = string.Empty;
            string[] lines = File.ReadAllLines(_configFilePath);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("#") || string.IsNullOrEmpty(line))     // comment or blank line
                    continue;
                else if (line.StartsWith("["))                              // section header
                    currentSection = line.Trim('[', ']').ToLower();
                else                                                        // regex or comic definition
                {
                    string[] parts = line.Split(',');
                    if (currentSection == Constants.Sections.Regex) // regex
                    {
                        var cr = new ComicStripRegex();
                        cr.Name = parts[0];
                        cr.Regex = parts[1];
                        _regexes.Add(cr);
                    }
                    else // comic
                    {
                        var c = new Comic();
                        c.Title = parts[0];
                        c.Url = parts[1];
                        if (parts[2].ToLower().StartsWith("rgx")) // rgxXXXXX variable
                        {
                            var regex = _regexes.Where(x => x.Name.ToLower() == parts[2].ToLower()).FirstOrDefault();
                            if (regex == null)
                            {
                                Logger.WriteLine("No regex with name {0}!", parts[2]);
                                return false;
                            }
                            c.SearchRegex = regex.Regex;
                        }
                        else
                            c.SearchRegex = parts[2]; // per-comic regex

                        _comics.Add(c);
                    }
                }
            }

            return true;
        }
    }
}
