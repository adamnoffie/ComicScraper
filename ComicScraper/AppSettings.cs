using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ComicScraper
{
    static class AppSettings
    {
        public static IConfigurationRoot Config { get; private set; }
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

            // If no appsettings.json exists in DataPath, seed it from the bundled default
            string dataAppSettings = Path.Combine(DataPath, "appsettings.json");
            string baseAppSettings = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.publish.json");
            if (!File.Exists(dataAppSettings) && File.Exists(baseAppSettings))
                File.Copy(baseAppSettings, dataAppSettings);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true) // developer overrides
                .AddJsonFile(Path.Combine(DataPath, "appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine(DataPath, "appsettings.local.json"), optional: true)
                .AddEnvironmentVariables();

            Config = builder.Build();

            UserAgent = Config["UserAgent"];

            HttpHeaders = [];
            var headersSection = Config.GetSection("HttpHeaders");
            foreach (var child in headersSection.GetChildren())
            {
                HttpHeaders[child.Key] = child.Value;
            }

            var emailUseSslValue = Config["EmailUseSSL"];
            EmailUseSSL = !bool.TryParse(emailUseSslValue, out var emailUseSsl) || emailUseSsl;
            EmailToAddresses = Config["EmailToAddresses"];
            SmtpFrom = Config["Smtp:From"];
            SmtpHost = Config["Smtp:Host"];
            SmtpPort = int.TryParse(Config["Smtp:Port"], out var smtpPort) ? smtpPort : 587;
            SmtpUserName = Config["Smtp:UserName"];
            SmtpPassword = Config["Smtp:Password"];
        }
    }
}
