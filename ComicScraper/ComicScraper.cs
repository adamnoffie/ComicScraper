using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Util;

namespace ComicScraper
{
    class ComicScraper
    {
        string _historyFilePath => Constants.ComicsHistoryFile;
        
        List<Comic> _comics = new List<Comic>();

        // simple record for deserializing history entries
        private record ComicHistory(string Title, string PreviousImgUrl, int PreviousImgSize); 

        /// <summary>
        /// Run the ComicScraper
        /// </summary>
        public void Run()
        {
            // 1. read in configuration file, and history file if there is one; set log file path
            if (!ReadConfigFile())
                return;
            ReadHistoryFile();
            Logger.LogFilePath = Constants.ComicsLogFile;
            Logger.WriteLine("Comic Scraper running...");

            // 2. create directory for storing images if it doesn't exist
            if (!Directory.Exists(Constants.ComicStripImgPath))
                Directory.CreateDirectory(Constants.ComicStripImgPath);

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
                Logger.WriteLine("   Sending email to {0}", AppSettings.EmailToAddresses);
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
            string pageHtml = HttpFetch.UrlAsString(c.Url, AppSettings.UserAgent, AppSettings.HttpHeaders);
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
                int comicSize = (int)HttpFetch.UrlContentSize(comicUri, AppSettings.UserAgent, AppSettings.HttpHeaders, c.Url);
                if (comicUri.ToString() != c.PreviousImgUrl || comicSize != c.PreviousImgSize) // different url or different size, download comic
                {
                    c.PreviousImgSize = comicSize;
                    c.PreviousImgUrl = comicUri.ToString();
                    string filePath = string.Format(Constants.ComicStripImgFilePath, c.Title);
                    c.StripImgFilePath = HttpFetch.UrlToFile(comicUri, filePath, AppSettings.UserAgent, AppSettings.HttpHeaders, c.Url);
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

                // save the downloaded HTML page for troubleshooting
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string debugFileName = $"{c.Title}_{timestamp}.html";
                string debugFilePath = Path.Combine(Constants.ComicStripImgPath, debugFileName);
                File.WriteAllText(debugFilePath, pageHtml);
                Logger.WriteLine("!!   Saved page HTML to {0} for troubleshooting.", debugFilePath);
            }
        }

        // write history file (json)
        private void WriteHistoryFile()
        {
            var historyData = _comics.Select(c => new { c.Title, c.PreviousImgUrl, c.PreviousImgSize }).ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(historyData, options);
            File.WriteAllText(_historyFilePath, json);
        }

        // see if we have a history file, with info about previous fetches of each comic, if existing
        private void ReadHistoryFile()
        {
            if (File.Exists(_historyFilePath))
            {
                // read in the info about comics in the history JSON file
                string json = File.ReadAllText(_historyFilePath);
                var histories = JsonSerializer.Deserialize<List<ComicHistory>>(json);
                foreach (var ch in histories)
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

        // Load comic definitions from appsettings.json configuration
        private bool ReadConfigFile()
        {
            // Load named regexes
            var regexes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var regexSection = AppSettings.Config.GetSection("Regexes");
            foreach (var child in regexSection.GetChildren())
            {
                regexes[child.Key] = child.Value;
            }

            // Load comics
            var comicsSection = AppSettings.Config.GetSection("Comics");
            foreach (var child in comicsSection.GetChildren())
            {
                var c = new Comic
                {
                    Title = child["Title"],
                    Url = child["Url"]
                };

                string regVal = child["Regex"];
                if (regVal.StartsWith("rgx", StringComparison.OrdinalIgnoreCase))
                {
                    if (!regexes.TryGetValue(regVal, out string regex))
                    {
                        Logger.WriteLine("!!   No regex with name {0}!", regVal);
                        return false;
                    }
                    c.SearchRegex = regex;
                }
                else
                {
                    c.SearchRegex = regVal;
                }

                _comics.Add(c);
            }

            if (_comics.Count == 0)
            {
                Logger.WriteLine("!!   No comics configured in appsettings! Exiting.");
                return false;
            }

            return true;
        }
    }
}
