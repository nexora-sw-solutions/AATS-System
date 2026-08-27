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
                var host = smtpSection["Host"] ?? "smtp.gmail.com";
                var portStr = smtpSection["Port"] ?? "587";
                var enableSslStr = smtpSection["EnableSsl"] ?? "true";
                var username = smtpSection["Username"];
                var password = smtpSection["Password"];
                var fromEmail = smtpSection["FromEmail"];
                var fromName = smtpSection["FromName"] ?? "AATS System";

                if (string.IsNullOrWhiteSpace(fromEmail)) fromEmail = username;
                if (string.IsNullOrWhiteSpace(fromEmail)) fromEmail = "nexora280@gmail.com";

                // Save email to local diagnostic folder as fallback/verification
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
                    Console.WriteLine($"[EmailService Diagnostic] Saved copy of email to: {filePath}");
                }
                catch (Exception fileEx)
                {
                    _logger.LogWarning(fileEx, "Failed to save a local diagnostic copy of the email.");
                }

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine($"[EmailService Warning] SMTP credentials (Username/Password) are not configured in appsettings.json. Email dispatch skipped. (Local copy saved to sent_emails)");
                    _logger.LogWarning("SMTP is not fully configured in appsettings.json. Email dispatch bypassed for recipient {To}.", to);
                    return;
                }

                if (!int.TryParse(portStr, out var port)) port = 587;
                if (!bool.TryParse(enableSslStr, out var enableSsl)) enableSsl = true;

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
                        smtpClient.Timeout = 15000; // 15 seconds timeout

                        _logger.LogInformation("Sending email to {Recipient} via SMTP host {Host}:{Port}...", to, host, port);
                        Console.WriteLine($"[EmailService] Sending email to {to} via {host}:{port}...");
                        await smtpClient.SendMailAsync(mailMessage);
                        Console.WriteLine($"[EmailService Success] Email successfully sent to {to}.");
                        _logger.LogInformation("Email sent successfully to {Recipient}.", to);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService Error] Failed to send email to {to}: {ex.Message}");
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            }
        }
    }
}
