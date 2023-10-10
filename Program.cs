using System.Net.Mail;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;


internal static class Program
{
    public static async Task Main(string[] args)
    {
        var stock = Cli.Parser.Parse(args);
        Util.Logger.Log($"Parsed CLI: Name = {stock}, SellPrice = {stock.SellPrice}, BuyPrice = {stock.BuyPrice}");

        var settings = Settings.Parser.Parse("appsettings.json");
        Util.Logger.Log($"Parsed settings: {settings}");


        using var queue = new BlockingCollection<MailMessage>();

        Console.CancelKeyPress += delegate
        {
            queue.CompleteAdding();
        };


        using var producer = Task.Run(() => Producer(settings.Api, stock, settings.Notify, queue));
        using var consumer = Task.Run(() => Consumer(settings.Host, queue));

        await Task.WhenAll(producer, consumer);
    }


    [DoesNotReturn]
    private static async Task Producer(Api.Info api, Stock.Info stock, Email.Notify notify, BlockingCollection<MailMessage> queue)
    {
        var client = new Api.Client(api);
        
        while (true)
        {
            var price = await client.Get(stock);

            if (price >= stock.SellPrice)
            {
                Util.Logger.Log($"Recommend selling since the price is {price:C2} >= {stock.SellPrice:C2}");

                foreach (var receiver in notify.Receivers)
                {
                    var message = Email.Message.Sell(notify.Sender, receiver, stock, price);
                    queue.Add(message);
                }
            }
            else if (price <= stock.BuyPrice)
            {
                Util.Logger.Log($"Recommend buying since the price is {price:C2} <= {stock.BuyPrice:C2}");
                
                foreach (var receiver in notify.Receivers)
                {
                    var message = Email.Message.Buy(notify.Sender, receiver, stock, price);
                    queue.Add(message);
                }
            }
            else
            {
                Util.Logger.Log($"Didn't recommend anything since price is {price:C2}");
            }
            
            Thread.Sleep((int)api.Period);
        }
    }

    private static async Task Consumer(Email.Host host, BlockingCollection<MailMessage> queue) {
        using var client = new Email.Client(host);

        while (true)
        {
            MailMessage message;

            try
            {
                message = queue.Take();
            }
            catch (InvalidOperationException)
            {
                // The queue concluded adding and is now empty
                return;
            }

            Util.Logger.Log($"Sending message: From = {message.From}, To = {message.To}, Subject = {message.Subject}");
            await client.Send(message);

            // Avoid passing the SMTP server rate limit
            Thread.Sleep((int)host.Period);
        }
    }
}
