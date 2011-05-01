
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
            // TODO: try catch

            HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
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

        /// <summary>
        /// Do an Http GET request of a url, and return the results as a string (good for HTML pages)
        /// </summary>
        /// <param name="url">url of object to fetch</param>
        /// <param name="userAgent">UserAgent http header</param>
        /// <returns></returns>
        public static string UrlAsString(string url, string userAgent)
        {
            // TODO: try catch, return null or string.Emtpy on error

            //
            // TODO: http://www.google.com/search?aq=f&sourceid=chrome&ie=UTF-8&q=WebRequest+C%23+read+page+as+string
            //      Make this more reliable using maybe a byte[] too, since that seems to be more reliable
            //

            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "GET";
            req.UserAgent = userAgent;
            using (WebResponse resp = req.GetResponse())
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                


                return sr.ReadToEnd();  // THIS STALLS SOMETIMES???

                //// THIS DOESN'T GET EVERYTHING ON SOME REQUESTS
                //StringWriter sw = new StringWriter();
                //char[] buffer = new char[32 * 1024];
                //int read = sr.Read(buffer, 0, buffer.Length);
                //while (read > 0)
                //{
                //    sw.Write(buffer);
                //    read = sr.Read(buffer, 0, buffer.Length);
                //}

                //return sw.ToString();
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
            // TODO: try catch

            HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
            req.Method = "HEAD";
            req.UserAgent = userAgent;
            req.Referer = referer;
            using (WebResponse resp = req.GetResponse())
            {
                return resp.ContentLength;
            }
        }

    }
}
