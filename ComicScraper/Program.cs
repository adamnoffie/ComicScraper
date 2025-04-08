using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ComicScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            ComicScraper cs = new ComicScraper();
            cs.Run();         
   
            // TODO: remove this???
            //Console.WriteLine("   DONE. Hit <ENTER> to quit...");
            //Console.Read();
        }
    }
}
