using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Util;
using ComicScraper.Properties;

namespace ComicScraper
{
    public static class ComicEmailer
    {
        public const int MaxAttempts = 3;

        /// <summary>
        /// Compose an email with comic images embedded, and send it to addresses specified in config file
        /// </summary>
        /// <param name="comics">Comics to send</param>
        public static void SendEmails(IEnumerable<Comic> comics)
        {
            MailMessage msg = new MailMessage();
            string subject = string.Format("Comics for {0:M/d/yyyy}", DateTime.Now);
            
            // build the string for html email
            StringBuilder emailHtmlBody = new StringBuilder();
            emailHtmlBody.AppendFormat("<h2>").Append(subject).Append("</h2>").AppendLine();
            foreach (Comic c in comics)
            {
                emailHtmlBody
                    .Append("<p style='font-family: Verdana, Georgia; font-size: 9pt'><strong>")
                    .Append(c.Title)
                    .Append("</strong><br /><a href='").Append(c.Url)
                    .Append("' target='_new'><img style='border: none' src='cid:").Append(c.ContentID)
                    .Append("' title='").Append(c.AltText)
                    .Append("' alt='").Append(c.ToolTip)
                    .Append("'/></a><br />").Append(c.AltText)
                    .Append("</p>").AppendLine();
            }
            emailHtmlBody.Append("<p style='font-family: Consolas, monospace; font-size: 8pt; color: #767676'>")
                .Append("--<br/>")
                .Append(Logger.SessionLog.Replace(Environment.NewLine, "<br/>"))
                .Append("</p>").AppendLine();
            
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(emailHtmlBody.ToString(),
                null, "text/html");

            // embed the images for the comics in the html view
            foreach (Comic c in comics)
            {
                if (!string.IsNullOrEmpty(c.StripImgFilePath))
                {
                    LinkedResource img = new LinkedResource(c.StripImgFilePath);
                    img.ContentId = c.ContentID;
                    htmlView.LinkedResources.Add(img);
                }
            }

            // plain text version of email
            AlternateView plainView = AlternateView.CreateAlternateViewFromString(
                "Your email viewer does not support HTML, which will make looking at this email pointless!",
                null, "text/plain");

            msg.AlternateViews.Add(plainView);
            msg.AlternateViews.Add(htmlView);

            // send the email!
            int attempts = 0;
            bool success = false;
            while (!success && attempts < MaxAttempts)
            {
                try
                {
                    EmailSender.SendEmail(msg, Settings.Default.EmailToAddresses, subject, Settings.Default.EmailUseSSL);
                    success = true;
                }
                catch (Exception ex)
                {
                    attempts++;
                    Logger.WriteLine("Exception: " + ex.Message);
                    Logger.WriteLine("Trying again...");
                }
            }
        }
    }
}
