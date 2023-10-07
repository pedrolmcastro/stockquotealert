// SmtpClient isn't recomended for new development since it doesn't support modern protocols
// It was used in this project because it fulfills our needs and avoids external dependencies

namespace Email;

using System.Net;
using System.Net.Mail;


internal readonly record struct Host(string Name, ushort Port);
internal readonly record struct Credential(string Username, string Password);


internal static class Message
{
    public static MailMessage Buy(
        MailAddress sender,
        MailAddress receiver,
        string asset,
        decimal price,
        decimal reference
    ) {
        return new(sender, receiver)
        {
            Subject = $"Buying {asset} is recomended",
            Headers = { { "Message-Id", $"<{Guid.NewGuid()}@{sender.Host}>" } },
            Body = 
                $"The monitored asset {asset} price is currently {price:C2}, which is lower" +
                $" than the reference price of {reference:C2}, so buying it is recommended."
        };
    }

    public static MailMessage Sell(
        MailAddress sender,
        MailAddress receiver,
        string asset,
        decimal price,
        decimal reference
    ) {
        return new(sender, receiver)
        {
            Subject = $"Selling {asset} is recomended",
            Body =
                $"The monitored asset {asset} price is currently {price:C2}, which is greater" +
                $" than the reference price of {reference:C2}, so selling it is recommended."
        };
    }
}


internal class Client : IDisposable
{
    private const int ErrorCode = 3;
    
    private readonly SmtpClient _client;


    public Client(Host host, Credential credential)
    {
        try
        {
            _client = new(host.Name, host.Port)
            {
                Credentials = new NetworkCredential(credential.Username, credential.Password),
                EnableSsl = true
            };
        }
        catch (Exception e)
        {
            Util.Error.Exit($"Failed to create SMTP client: {e.Message}", ErrorCode);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }


    public void Send(MailMessage message)
    {
        try
        {
            _client.Send(message);
        }
        catch (Exception e)
        {
            Util.Error.Exit($"Failed to send email: {e.Message}", ErrorCode);
        }
    }
}
