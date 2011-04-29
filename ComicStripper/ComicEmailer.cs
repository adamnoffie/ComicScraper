﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Util;
using ComicStripper.Properties;

namespace ComicStripper
{
    public static class ComicEmailer
    {
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
                    .Append("<p style='font-face: Verdana, Georgia; font-size: 11px'><strong>")
                    .Append(c.Title)
                    .Append("</strong><br /><a href='").Append(c.Url)
                    .Append("' target='_new'><img style='border: none' src='cid:").Append(c.ContentID)
                    .Append("' title='").Append(c.AltText)
                    .Append("' alt='").Append(c.ToolTip)
                    .Append("'/></a><br />").Append(c.AltText)
                    .Append("</p>").AppendLine();
            }
            
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(emailHtmlBody.ToString(),
                null, "text/html");

            // embed the images for the comics in the html view
            foreach (Comic c in comics)
            {
                LinkedResource img = new LinkedResource(c.StripImgFilePath);
                img.ContentId = c.ContentID;
                htmlView.LinkedResources.Add(img);
            }

            // plain text version of email
            AlternateView plainView = AlternateView.CreateAlternateViewFromString(
                "Your email viewer does not support HTML, which will make looking at this email pointless!",
                null, "text/plain");

            // send the email!
            msg.AlternateViews.Add(plainView);
            msg.AlternateViews.Add(htmlView);
            EmailSender.SendEmail(msg, Settings.Default.EmailToAddresses, subject, Settings.Default.EmailUseSSL);
        }
    }
}
