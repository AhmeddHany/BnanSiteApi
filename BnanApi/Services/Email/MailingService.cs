using BnanApi.DTOS;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BnanApi.Services.Email
{
    public class MailingService : IMailingService
    {
        private readonly MailSettings _mailSettings;

        public MailingService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        // This To Send Message For Customer To Ressure OrDer 
        public async Task<bool> SendEmailForCustomer(string emailCustomer, string userName)
        {

            // Email sending logic here
            string SubjectAr = "طلب النسخة التجريبية لنظام بنان";
            string bodyAr = $"عزيزي, {userName}،\n تم إرسال طلبك بنجاح، وسيتم التواصل معك عبر البريد من خلال خدمة العملاء. ";

            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.Email),
                Subject = SubjectAr
            };

            email.To.Add(MailboxAddress.Parse(emailCustomer));

            var builder = new BodyBuilder();
            builder.HtmlBody = bodyAr;
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));

            var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Email, _mailSettings.Password);
            await smtp.SendAsync(email);

            smtp.Disconnect(true);
            return true; // Email sent successfully


        }

        public async Task<bool> SendEmailToBnan(EmailDTO request)
        {
            string SubjectAr = "طلب النسخة التجريبية لنظام بنان";
            // Email sending logic here
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.Email),
                Subject = SubjectAr
            };

            email.To.Add(MailboxAddress.Parse(_mailSettings.Email));

            var builder = new BodyBuilder();
            builder.HtmlBody = "";

            // Add name if exists
            if (!string.IsNullOrEmpty(request.Name))
            {
                builder.HtmlBody += $"<p>Name: {request.Name}</p>";
            }

            // Add phone number if exists
            if (!string.IsNullOrEmpty(request.Phone))
            {
                builder.HtmlBody += $"<p>Phone Number: {request.Phone}</p>";
            }

            // Add email, city, and district if they exist
            if (!string.IsNullOrEmpty(request.Email))
            {
                builder.HtmlBody += $"<p>Email: {request.Email}</p>";
            }

            if (!string.IsNullOrEmpty(request.City))
            {
                builder.HtmlBody += $"<p>City: {request.City}</p>";
            }

            if (!string.IsNullOrEmpty(request.District))
            {
                builder.HtmlBody += $"<p>District: {request.District}</p>";
            }
            if (!string.IsNullOrEmpty(request.Body))
            {
                builder.HtmlBody += request.Body;
            }

            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));

            var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Email, _mailSettings.Password);
            await smtp.SendAsync(email);

            smtp.Disconnect(true);

            return true; // Email sent successfully

        }

        public bool IsValidEmail(string email)
        {

            var addr = new MailAddress(email);
            if (addr != null) return addr.Address == email;
            return false;

        }
    }
}
