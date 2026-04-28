using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ComicScraper
{
    static class AppSettings
    {
        public static string UserAgent { get; private set; }
        public static Dictionary<string, string> HttpHeaders { get; private set; }
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

            HttpHeaders = new Dictionary<string, string>();
            var headersSection = config.GetSection("HttpHeaders");
            foreach (var child in headersSection.GetChildren())
            {
                HttpHeaders[child.Key] = child.Value;
            }

            var emailUseSslValue = config["EmailUseSSL"];
            EmailUseSSL = bool.TryParse(emailUseSslValue, out var emailUseSsl) ? emailUseSsl : true;
            EmailToAddresses = config["EmailToAddresses"];
            SmtpFrom = config["Smtp:From"];
            SmtpHost = config["Smtp:Host"];
            SmtpPort = int.TryParse(config["Smtp:Port"], out var smtpPort) ? smtpPort : 587;
            SmtpUserName = config["Smtp:UserName"];
            SmtpPassword = config["Smtp:Password"];
        }
    }
}
