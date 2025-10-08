// EmailSender.cs
using System.Net;
using System.Net.Mail;

public class EmailSender
{
    // -> Werte an deinen SMTP-Server anpassen
    private const string Host = "smtp.gmail.com";
    private const int Port = 587;
    private const string User = "simon.freygang@gmail.com";
    private const string Pass = "xxx";
    private const string From = "simon.freygang@gmail.com";

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        using var smtp = new SmtpClient(Host, Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(User, Pass)
        };

        using var msg = new MailMessage(From, to)
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        //await smtp.SendMailAsync(msg);
    }
}
