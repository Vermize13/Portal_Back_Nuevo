using Microsoft.Extensions.Options;
using Resend;
using API.DTOs;

namespace API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly EmailSettings _emailSettings;

        public EmailService(IResend resend, IOptions<EmailSettings> emailSettings)
        {
            _resend = resend;
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string token)
        {
            try
            {
                var resetLink = $"{_emailSettings.FrontendUrl}/login/reestablecer/{token}";
                
                var message = new EmailMessage();
                message.From = new EmailAddress { Email = _emailSettings.FromEmail, DisplayName = _emailSettings.FromName };
                message.To.Add(email);
                message.Subject = "Restablecimiento de Contraseña - Martiniere Bug System";
                message.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Solicitud de restablecimiento de contraseña</h2>
                        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en Martiniere Bug System.</p>
                        <p>Para continuar, haz clic en el siguiente botón:</p>
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Restablecer Contraseña</a>
                        </p>
                        <p>O copia y pega el siguiente enlace en tu navegador:</p>
                        <p style='background-color: #f3f4f6; padding: 10px; word-break: break-all; font-size: 14px;'>{resetLink}</p>
                        <p>Este enlace expirará en 24 horas.</p>
                        <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                    </div>
                ";

                await _resend.EmailSendAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
