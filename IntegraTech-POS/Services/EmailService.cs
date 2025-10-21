using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace IntegraTech_POS.Services
{
    public class EmailService
    {
        private readonly DatabaseService _db;

        public EmailService(DatabaseService db)
        {
            _db = db;
        }

    public async Task<bool> EnviarAsync(string toEmail, string subject, string bodyText, IEnumerable<string>? attachmentPaths = null, string? ccEmails = null, string? bccEmails = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail)) return false;

                
                var host = await _db.ObtenerConfiguracionAsync("SMTP_HOST");
                var portStr = await _db.ObtenerConfiguracionAsync("SMTP_PORT");
                var user = await _db.ObtenerConfiguracionAsync("SMTP_USER");
                var pass = await _db.ObtenerConfiguracionAsync("SMTP_PASS");
                var from = await _db.ObtenerConfiguracionAsync("SMTP_FROM") ?? user ?? "noreply@local";
                var useSslStr = await _db.ObtenerConfiguracionAsync("SMTP_SSL");

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                {
                    Console.WriteLine("Correo no configurado (faltan SMTP_*). Skipping send.");
                    return false;
                }

                int port = 587;
                if (!string.IsNullOrWhiteSpace(portStr) && int.TryParse(portStr, out var p)) port = p;
                bool useSsl = true;
                if (!string.IsNullOrWhiteSpace(useSslStr) && bool.TryParse(useSslStr, out var b)) useSsl = b;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("IntegraTech POS", from));
                message.To.Add(MailboxAddress.Parse(toEmail));
                
                if (!string.IsNullOrWhiteSpace(ccEmails))
                {
                    foreach (var cc in ccEmails.Split(new[] {',',';'}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        message.Cc.Add(MailboxAddress.Parse(cc));
                    }
                }
                if (!string.IsNullOrWhiteSpace(bccEmails))
                {
                    foreach (var bcc in bccEmails.Split(new[] {',',';'}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        message.Bcc.Add(MailboxAddress.Parse(bcc));
                    }
                }
                message.Subject = subject;

                var builder = new BodyBuilder { TextBody = bodyText };
                if (attachmentPaths != null)
                {
                    foreach (var path in attachmentPaths)
                    {
                        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                        {
                            builder.Attachments.Add(path);
                        }
                    }
                }
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                
                SecureSocketOptions secure = SecureSocketOptions.Auto;
                if (useSsl)
                {
                    secure = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                }

                try
                {
                    await client.ConnectAsync(host, port, secure);
                }
                catch
                {
                    
                    await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
                }

                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                try { await _db.GuardarConfiguracionAsync("SMTP_LAST_ERROR", ex.ToString(), "Ãšltimo error SMTP"); } catch { }
                return false;
            }
        }
    }
}

