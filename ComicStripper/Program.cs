using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ComicStripper
{
    class Program
    {
        static void Main(string[] args)
        {
            ComicStripper cs = new ComicStripper();
            cs.Run();         
   
            // TODO, remove this:
            Console.Read();
        }
    }
}
