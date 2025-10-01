using AutoPartsStore.Core.Interfaces.IServices.IEmailSirvices;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AutoPartsStore.Infrastructure.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationCodeAsync(string toEmail, string code)
        {
            var smtpSettings = _configuration.GetSection("Smtp");

            var smtpClient = new SmtpClient(smtpSettings["Host"])
            {
                Port = int.Parse(smtpSettings["Port"]),
                Credentials = new NetworkCredential(
                smtpSettings["Username"],
                smtpSettings["Password"]
            ),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings["Username"]),
                Subject = "رمز التحقق الخاص بك",
                Body = $"رمز التحقق الخاص بك هو: {code}\n\nهذا الرمز صالح لمدة 2 دقائق.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
