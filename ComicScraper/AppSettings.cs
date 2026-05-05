using System;
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
        public static string DataPath { get; private set; }

        public static void Load()
        {
            // Resolve DataPath early from env var (set by Dockerfile ENV) so we
            // can look for config file overrides in it. Falls back to CWD.
            DataPath = Environment.GetEnvironmentVariable("DataPath") ?? Directory.GetCurrentDirectory();
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(Path.Combine(DataPath, "appsettings.json"), optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddJsonFile(Path.Combine(DataPath, "appsettings.local.json"), optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

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
