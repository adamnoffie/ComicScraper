
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Util;

namespace Util
{
    public static class HttpFetch
    {
        public const int MaxAttempts = 3;
        public const int DefaultTimeout = 10 * 1000; // 10 seconds


        /// <summary>
        /// Do an Http GET of a url, and store results to file. Extension is determined by content type.
        /// </summary>
        /// <param name="uri">uri of object to fetch</param>
        /// <param name="filePath">file path to store fetched object at (this method will try to determine the correct file extension)</param>
        /// <param name="userAgent">http UserAgent header</param>
        /// <param name="referer">http Referer header</param>
        /// <returns></returns>
        public static string UrlToFile(Uri uri, string filePath, string userAgent, string referer)
        {
            string returnVal = null;
            int attemptCounter = 1;
            while (returnVal == null && attemptCounter <= 3)
            {
                attemptCounter++;
                returnVal = DoFetch_UrlToFile(uri, filePath, userAgent, referer);
            }
            return returnVal;
        }

        private static string DoFetch_UrlToFile(Uri uri, string filePath, string userAgent, string referer)
        {
            try
            {
                HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
                req.ReadWriteTimeout = req.Timeout = DefaultTimeout;
                req.Method = "GET";
                req.UserAgent = userAgent;
                req.Referer = referer;
                using (WebResponse resp = req.GetResponse())
                using (Stream responseStream = resp.GetResponseStream())
                {
                    // file extension
                    string ext = ImageType.GetExtension(resp.ContentType);
                    filePath += "." + ext;

                    using (FileStream fs = File.Create(filePath))
                    {
                        // read 32K of the stream at a time, writing to file
                        Byte[] buffer = new Byte[32 * 1024];
                        int read = responseStream.Read(buffer, 0, buffer.Length);
                        while (read > 0)
                        {
                            fs.Write(buffer, 0, read);
                            read = responseStream.Read(buffer, 0, buffer.Length);
                        }
                    }

                    return filePath;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Do an Http GET request of a url, and return the results as a string (good for HTML pages)
        /// </summary>
        /// <param name="url">url of object to fetch</param>
        /// <param name="userAgent">UserAgent http header</param>
        /// <returns></returns>
        public static string UrlAsString(string url, string userAgent)
        {
            string returnVal = null;
            int attemptCounter = 1;
            while (returnVal == null && attemptCounter <= MaxAttempts)
            {
                attemptCounter++;
                returnVal = DoFetch_UrlAsString(url, userAgent);
            }
            return returnVal;
        }

        private static string DoFetch_UrlAsString(string url, string userAgent)
        {
            try
            {
                HttpWebRequest req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
                req.ReadWriteTimeout = req.Timeout = DefaultTimeout;
                req.Method = "GET";
                req.UserAgent = userAgent;
                if (url.Contains("https"))
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12
                        | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    
                using (WebResponse resp = req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Do an Http HEAD request to get the size of an object without fetching it
        /// </summary>
        /// <param name="uri">url to the resource</param>
        /// <param name="referer">Referer http header</param>
        /// <param name="userAgent">UserAgent http header</param>
        /// <returns></returns>
        public static long UrlContentSize(Uri uri, string userAgent, string referer)
        {
            int attemptCounter = 1;
            long returnVal = -1;
            while (returnVal == -1 && attemptCounter <= MaxAttempts)
            {
                attemptCounter++;
                returnVal = DoFetch_UrlContentSize(uri, userAgent, referer);
            }

            return returnVal;
        }

        private static long DoFetch_UrlContentSize(Uri uri, string userAgent, string referer)
        {
            try
            {
                HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
                req.ReadWriteTimeout = req.Timeout = DefaultTimeout;
                req.Method = "HEAD";
                req.UserAgent = userAgent;
                req.Referer = referer;
                using (WebResponse resp = req.GetResponse())
                {
                    return resp.ContentLength;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

    }
}
