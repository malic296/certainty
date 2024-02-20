using Azure.Core;
using Azure;
using System;
using System.Net;
using System.Net.Mail;


namespace certainty.Injections
{
    public interface IEmailSenderService
    {
        Task SendEmail(string email, string subject, string message);
        string CreateCode();
    }

    public class EmailSenderService : IEmailSenderService
    {
        public Task SendEmail(string email, string subject, string message)
        {
            try
            {
                var mail = "certaintyemail@gmail.com";
                var password = "kaym arfx oqah urvp";

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(mail, password)
                };

                return client.SendMailAsync(
                    new MailMessage(
                        from: mail,
                        to: email,
                        subject,
                        message
                    ));
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
             

        }

        public string CreateCode()
        {

            Random rnd = new Random();

            int code = rnd.Next(100000, 1000000);

            return code.ToString();
        }

        

    }
}
