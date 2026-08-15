using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AATS.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AATS.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSection = _configuration.GetSection("Smtp");
                var host = smtpSection["Host"];
                var portStr = smtpSection["Port"];
                var enableSslStr = smtpSection["EnableSsl"];
                var username = smtpSection["Username"];
                var password = smtpSection["Password"];
                var fromEmail = smtpSection["FromEmail"] ?? "nexora280@gmail.com";
                var fromName = smtpSection["FromName"] ?? "AATS System";

                // Save email to a local temp folder as fallback/diagnostic verification
                try
                {
                    var tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sent_emails");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}.html";
                    var filePath = Path.Combine(tempDir, fileName);
                    await File.WriteAllTextAsync(filePath, body);
                    _logger.LogInformation("Saved a diagnostic copy of the email to local file: {FilePath}", filePath);
                }
                catch (Exception fileEx)
                {
                    _logger.LogWarning(fileEx, "Failed to save a local diagnostic copy of the email.");
                }

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("SMTP is not fully configured in appsettings.json. Email dispatch bypassed. Logged email body: {Body}", body);
                    return;
                }

                if (!int.TryParse(portStr, out var port))
                {
                    port = 587;
                }

                if (!bool.TryParse(enableSslStr, out var enableSsl))
                {
                    enableSsl = true;
                }

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(fromEmail, fromName);
                    mailMessage.To.Add(to);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(host, port))
                    {
                        smtpClient.Credentials = new NetworkCredential(username, password);
                        smtpClient.EnableSsl = enableSsl;

                        _logger.LogInformation("Sending password reset email to {Recipient} via SMTP host {Host}.", to, host);
                        await smtpClient.SendMailAsync(mailMessage);
                        _logger.LogInformation("Password reset email sent successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email due to SMTP connection or credentials error. Swallowing exception to prevent application crash.");
            }
        }
    }
}
