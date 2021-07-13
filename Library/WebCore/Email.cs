using Microsoft.Extensions.Options;
using Spire.Email;
using Spire.Email.IMap;
using Spire.Email.Smtp;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;
using WebInterface.Settings;

namespace WebCore
{
    public class EmailTools : IEmailTools
    {
        private readonly SmtpSettings options;

        public EmailTools(IOptions<SmtpSettings> options) => this.options = options.Value;

        public Task SendEmailAsync(string email, string subject, string content, bool html = true, params string[] cc)
        {
            SmtpClient smtp = new SmtpClient
            {
                Host = options.Host,
                ConnectionProtocols = ConnectionProtocols.Ssl,
                Username = options.Username,
                Password = options.Password,
                Port = options.Port,
                AccessToken = options.AccessToken,
                UseOAuth = options.UseOAuth,
                Encoding = options.Encoding,
            };

            MailMessage message = new MailMessage(options.Username, email);

            foreach (string c in cc) message.Cc.Add(c);

            message.Date = DateTime.Now;
            message.Subject = subject;

            if (html)
            {
                message.BodyHtml = content;
            }
            else
            {
                message.BodyText = content;
                message.BodyTextCharset = options.Encoding;
            }

            if (options.TimeOut > 0) smtp.TimeOut = options.TimeOut;
            var cancellation = options.TimeOut > 0 ? new CancellationTokenSource(options.TimeOut) : new CancellationTokenSource();

            return Task.Factory.StartNew(() => smtp.SendOne(message), cancellation.Token);
        }
    }
}
