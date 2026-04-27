namespace ComicScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            AppSettings.Load();

            ComicScraper cs = new ComicScraper();
            cs.Run();
        }
    }
}
