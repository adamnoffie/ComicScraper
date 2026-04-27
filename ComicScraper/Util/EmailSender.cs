using System;
using System.Net;
using System.Net.Mail;

namespace Util
{
    public static class EmailSender
    {
        /// <summary>
        /// Sends an email using explicitly provided SMTP settings.
        /// </summary>
        /// <param name="msg">Must have the body or AlternateViews set</param>
        /// <param name="to">who to send email to - comma or semicolon separated</param>
        /// <param name="subject">email message subject</param>
        /// <param name="useSSL">use SSL to connect to SMTP server?</param>
        /// <param name="smtpHost">SMTP server hostname</param>
        /// <param name="smtpPort">SMTP server port</param>
        /// <param name="smtpFrom">From email address</param>
        /// <param name="smtpUserName">SMTP username</param>
        /// <param name="smtpPassword">SMTP password</param>
        public static void SendEmail(MailMessage msg, string to, string subject, bool useSSL,
            string smtpHost, int smtpPort, string smtpFrom, string smtpUserName, string smtpPassword)
        {
            msg.From = new MailAddress(smtpFrom);
            msg.Subject = subject;

            string[] tos = to.Split(',', ';');
            foreach (string address in tos)
                msg.To.Add(new MailAddress(address.Trim()));

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.EnableSsl = useSSL;
                smtp.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                smtp.Send(msg);
            }
        }
    }
}
