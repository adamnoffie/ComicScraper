using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ComicStripper.Properties;
using System.Threading;

namespace ComicStripper
{
    class ComicStripper
    {
        string _emailBody = "";
        string _configFilePath = Path.Combine(Environment.CurrentDirectory, Constants.ComicsFile);
        
        List<ComicStripRegex> _regexes = new List<ComicStripRegex>();
        List<Comic> _comics = new List<Comic>();

        /// <summary>
        /// Run the ComicStripper
        /// </summary>
        public void Run()
        {
            if (!ReadConfigFile())
                return;

            foreach (Comic c in _comics)
            {
                RipComic(c);
                Thread.Sleep(5 * 1000);
            }
        }

        // rip a comic from the site
        private void RipComic(Comic c)
        {
            Logger.WriteLine("== Stripping {0} == {1} ==", c.Title, c.Url);

            // TODO: read "ini file" for prevoius fetching of comics info

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

                long comicSize = FetchUrlContentSize(comicUri, c.Url);
                // setup spoof headers, request the strip image's size
                Logger.WriteLine("Size: " + comicSize);
                

                
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
