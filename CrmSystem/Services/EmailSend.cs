using System;
using System.Net;
using System.Net.Mail;

namespace CrmSystem.EmailSend
{
    public class EmailService
    {
        private readonly string smtpHost = "smtp.mail.ru";
        private readonly int smtpPort = 587;
        private readonly string smtpUser = "k060412@list.ru";
        private readonly string smtpPass = "HittSO48vothqL2617a5";

        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                using var smtp = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки email: {ex.Message}");
                return false;
            }
        }
    }

}




