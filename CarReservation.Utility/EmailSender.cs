using MailKit;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace CarReservation.Utility
{
    /// <summary>
    /// Implements the <see cref="IEmailSender"/> interface to send emails using MailKit.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        /// <summary>
        /// The configuration instance used to retrieve email settings.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="config">The configuration object containing email settings.</param>
        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Sends an email asynchronously using the configured SMTP settings.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The HTML content of the email.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="Exception">Thrown if the email sending process fails.</exception>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPortText = _config["EmailSettings:SmtpPort"];
            var smtpEmail = _config["EmailSettings:SmtpEmail"];
            var smtpUser = _config["EmailSettings:SmtpUser"];
            var smtpPass = _config["EmailSettings:SmtpPass"];

            if (string.IsNullOrWhiteSpace(smtpServer))
                throw new InvalidOperationException("EmailSettings:SmtpServer is missing.");

            if (!int.TryParse(smtpPortText, out var smtpPort))
                throw new InvalidOperationException("EmailSettings:SmtpPort is invalid.");

            if (string.IsNullOrWhiteSpace(smtpEmail))
                throw new InvalidOperationException("EmailSettings:SmtpEmail is missing.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Reservation System", smtpEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient(new ProtocolLogger(Console.OpenStandardOutput()));

            try
            {
                await client.ConnectAsync(smtpServer.Trim(), smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed. Host='{smtpServer}', Port={smtpPort}, Error={ex.Message}");
                throw;
            }
        }
    }
}
