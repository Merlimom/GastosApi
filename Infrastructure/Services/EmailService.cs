using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string token, string resetUrl)
    {
        var body = $"Para restablecer tu contraseña, haz clic en el siguiente enlace: {resetUrl}?token={token}";
        await SendEmailAsync(toEmail, "Restablecer contraseña", body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
        {
            Port = int.Parse(_configuration["Smtp:Port"]),
            Credentials = new NetworkCredential(_configuration["Smtp:User"], _configuration["Smtp:Pass"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Smtp:FromAddress"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // Este valor puede ser true o false dependiendo de si deseas enviar el cuerpo en HTML.
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
