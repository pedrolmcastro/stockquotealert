// SmtpClient isn't recomended for new development since it doesn't support modern protocols
// It was used in this project because it fulfills our needs and avoids external dependencies

namespace Email;

using System.Net;
using System.Net.Mail;
using System.Reflection;


internal readonly record struct Credential(string Username, string Password);
internal readonly record struct Host(string Name, ushort Port, Credential Credential, uint Period);


internal readonly record struct Notify(MailAddress Sender, List<MailAddress> Receivers);


internal static class Message
{
    public static MailMessage Buy(MailAddress sender, MailAddress receiver, Stock.Info stock, decimal price) {
        return new(sender, receiver)
        {
            Headers = { { "Message-Id", Id(sender) } },
            Subject = $"Buying {stock} is recomended!",
            Body =
                $"The monitored stock {stock} price is currently {price:C2}, which is lower " +
                $"than the reference price of {stock.BuyPrice:C2}, so buying it is recommended."
        };
    }

    public static MailMessage Sell(MailAddress sender, MailAddress receiver, Stock.Info stock, decimal price) {
        return new(sender, receiver)
        {
            Headers = { { "Message-Id", Id(sender) } },
            Subject = $"Selling {stock} is recomended!",
            Body =
                $"The monitored asset {stock} price is currently {price:C2}, which is greater " +
                $"than the reference price of {stock.SellPrice:C2}, so selling it is recommended."
        };
    }


    private static string Id(MailAddress sender) => $"<{Guid.NewGuid()}@{sender.Host}>";
}


internal class Client : IDisposable
{
    public const int ErrorCode = 3;
    
    private readonly SmtpClient _client;


    public Client(Host host)
    {
        try
        {
            _client = new(host.Name, host.Port)
            {
                Credentials = new NetworkCredential(host.Credential.Username, host.Credential.Password),
                EnableSsl = true
            };
        }
        catch (Exception exception)
        {
            Util.Error.Exit($"Failed to create SMTP client: {exception.Message}", ErrorCode);
        }
    }

    public void Dispose() => _client.Dispose();


    public Task Send(MailMessage message) => _client.SendMailAsync(message);
}
