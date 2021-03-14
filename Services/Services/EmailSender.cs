using Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace Services
{
    public class EmailSender : IEmailSender, ISingletonDependency
    {
        private readonly MailSettings mailSettings;
        private readonly IHostingEnvironment _env;

        public EmailSender(
            IOptions<SiteSettings> siteSettings,
            IHostingEnvironment env)
        {
            this.mailSettings = siteSettings.Value.MailSettings;
            _env = env;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            //var email = new MimeMessage();
            //var sender = MailboxAddress.Parse(mailSettings.Mail);
            //sender.Name = mailSettings.DisplayName;
            //email.Sender = sender;
            //email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            //email.Subject = mailRequest.Subject;
            //var builder = new BodyBuilder();
            //if (mailRequest.Attachments != null)
            //{
            //    byte[] fileBytes;
            //    foreach (var file in mailRequest.Attachments)
            //    {
            //        if (file.Length > 0)
            //        {
            //            using (var ms = new MemoryStream())
            //            {
            //                file.CopyTo(ms);
            //                fileBytes = ms.ToArray();
            //            }
            //            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
            //        }
            //    }
            //}
            //builder.HtmlBody = mailRequest.Body;
            //email.Body = builder.ToMessageBody();
            //using var smtp = new SmtpClient();
            //smtp.Connect(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
            //smtp.Authenticate(mailSettings.Mail, mailSettings.Password);
            //await smtp.SendAsync(email);
            //smtp.Disconnect(true);
        }
    }
}
