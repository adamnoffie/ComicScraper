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
        string _configFilePath = Path.Combine(Environment.CurrentDirectory, Constants.ComicsFile);
        string _historyFilePath = Path.Combine(Environment.CurrentDirectory, Constants.ComicsHistoryFile);
        
        List<ComicStripRegex> _regexes = new List<ComicStripRegex>();
        List<Comic> _comics = new List<Comic>();

        /// <summary>
        /// Run the ComicStripper
        /// </summary>
        public void Run()
        {
            // 1. read in configuration file, and history file if there is one; set log file path
            if (!ReadConfigFile())
                return;
            ReadHistoryFile();
            Logger.LogFilePath = Path.Combine(Environment.CurrentDirectory, Constants.ComicsLogFile);

            // 2. create directory for storing images if it doesn't exist
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, Constants.ComicStripImgPath)))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, Constants.ComicStripImgPath));

            // 3. strip the comics from the sites, storing to disk
            foreach (Comic c in _comics)
            {
                RipComic(c);
                Thread.Sleep(1 * 1000);
            }            

            // 4. compose an email with new comic images embedded, and send it to addresses specified in config file
            var newComics = _comics.Where(x => x.IsNewComic);
            if (newComics.Count() > 0)
            {
                Logger.WriteLine("   Sending email to {0}", Settings.Default.EmailToAddresses);
                ComicEmailer.SendEmails(newComics);
            }
            else
                Logger.WriteLine("!!   No new comics, skipping email!");

            // 5. write the history file back out (with new PreviousImgSize and PreviousImgUrl values of _comics)
            WriteHistoryFile();
        }

        // rip a comic from the site
        private void RipComic(Comic c)
        {
            Logger.WriteLine("   Stripping {0} ", c.Title);

            // get the page that has the comic
            string pageHtml = HttpFetch.UrlAsString(c.Url, Settings.Default.UserAgent);
            if (pageHtml == null)
            {
                Logger.WriteLine("!!   Http Fetch for page failed!");
                return;
            }

            // get the comic image url
            var r = new Regex(c.SearchRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match = r.Match(pageHtml);

            if (match.Success && match.Groups["url"] != null && match.Groups["url"].Success)
            {
                string url = match.Groups["url"].Value;
                // this will handle the comic strip URL being absolute OR relative, and give you an absolute in the end
                Uri comicUri = new Uri(new Uri(c.Url), new Uri(url, UriKind.RelativeOrAbsolute));                
                c.ToolTip = match.Groups["title"].Value;
                c.AltText = match.Groups["alt"].Value;

                // quickly get (using HEAD) the strip image's size, then see if it is different comic
                // NOTE: this assumes different size is different comic, same size is same comic
                // I'm assuming it is very unlikely that two strips in a row for same comic would be exact same size in bytes
                int comicSize = (int)HttpFetch.UrlContentSize(comicUri, Settings.Default.UserAgent, c.Url);
                if (comicUri.ToString() != c.PreviousImgUrl || comicSize != c.PreviousImgSize) // different url or different size, download comic
                {
                    c.PreviousImgSize = comicSize;
                    c.PreviousImgUrl = comicUri.ToString();
                    string filePath = Path.Combine(Environment.CurrentDirectory, string.Format(Constants.ComicStripImgFilePath, c.Title));
                    c.StripImgFilePath = HttpFetch.UrlToFile(comicUri, filePath, Settings.Default.UserAgent, c.Url);
                }
                else // same size, same comic as last run, don't need to fetch
                {
                    Logger.WriteLine("     Same url and size as Previous Fetch. Skipping.");
                    c.IsNewComic = false;
                }
            }
            else // regex matched nothing, or didn't find the "url" group!!!
            {
                Logger.WriteLine("!!   Regex could not find the strip image!");
            }
        }

        // write history file (xml)
        private void WriteHistoryFile()
        {
            _comics.ToXmlFile(_historyFilePath);
        }

        // see if we have a history file, with info about previous fetches of each comic, if existing
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

        // Read the Comics.txt configuration file to get regexes and comics to strip
        private bool ReadConfigFile()
        {
            if (!File.Exists(_configFilePath))
            {
                Logger.WriteLine("!!   No Comic Config File (Comics.txt)! Exiting.");
                return false;
            }

            string currentSection = string.Empty;
            string[] lines = File.ReadAllLines(_configFilePath);

            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    string line = lines[i].Trim();

                    if (line.StartsWith("#") || string.IsNullOrEmpty(line))     // comment or blank line
                        continue;
                    else if (line.StartsWith("["))                              // section header
                        currentSection = line.Trim('[', ']').ToLower();
                    else                                                        // regex or comic definition
                    {
                        string[] parts;
                        if (currentSection == Constants.Sections.Regex) // regex
                        {
                            parts = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            var cr = new ComicStripRegex {
                                Name = parts[0].Trim(),
                                Regex = parts[1].Trim()
                            };
                            _regexes.Add(cr);
                        }
                        else // comic
                        {
                            parts = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var c = new Comic {
                                Title = parts[0].Trim(),
                                Url = parts[1].Trim()
                            };
                            string regVal = parts[2].Trim();
                            if (regVal.ToLower().StartsWith("rgx")) // rgxXXXXX variable
                            {
                                var regex = _regexes.Where(x => x.Name.ToLower() == regVal.ToLower()).FirstOrDefault();
                                if (regex == null)
                                {
                                    Logger.WriteLine("!!   No regex with name {0}!", regVal);
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
                catch (Exception e)
                {
                    Logger.WriteLine("!!   Error reading config file [line {0}]: Exception {1}: {2}", i + 1, e.ToString(), e.Message);
                }
            }

            return true;
        }
    }
}
