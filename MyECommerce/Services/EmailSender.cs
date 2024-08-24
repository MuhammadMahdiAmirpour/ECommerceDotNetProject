using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MyECommerce.Configuration;

namespace MyECommerce.Services;

public class EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger) : IEmailSender {
	private readonly EmailSettings _emailSettings = emailSettings.Value;

	public async Task SendEmailAsync(string email, string subject, string message) {
		try {
			using var client = new SmtpClient(_emailSettings.MailServer, _emailSettings.MailPort);
			if (_emailSettings.SenderEmail != null) {
				var mailMessage = new MailMessage {
					From       = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
					Subject    = subject,
					Body       = message,
					IsBodyHtml = true,
				};
				mailMessage.To.Add(email);

				await client.SendMailAsync(mailMessage);
			}
			logger.LogInformation($"Email sent successfully to {email}");
		} catch (Exception ex) {
			logger.LogError(ex, $"Error sending email to {email}");
			throw;
		}
	}
}
