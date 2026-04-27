using System.IO;
using Microsoft.Extensions.Configuration;

namespace ComicScraper
{
    static class AppSettings
    {
        public static string UserAgent { get; private set; }
        public static bool EmailUseSSL { get; private set; }
        public static string EmailToAddresses { get; private set; }
        public static string SmtpFrom { get; private set; }
        public static string SmtpHost { get; private set; }
        public static int SmtpPort { get; private set; }
        public static string SmtpUserName { get; private set; }
        public static string SmtpPassword { get; private set; }

        public static void Load()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            UserAgent = config["UserAgent"];
            EmailUseSSL = bool.Parse(config["EmailUseSSL"] ?? "true");
            EmailToAddresses = config["EmailToAddresses"];
            SmtpFrom = config["Smtp:From"];
            SmtpHost = config["Smtp:Host"];
            SmtpPort = int.Parse(config["Smtp:Port"] ?? "587");
            SmtpUserName = config["Smtp:UserName"];
            SmtpPassword = config["Smtp:Password"];
        }
    }
}
