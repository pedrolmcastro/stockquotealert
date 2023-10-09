using System.Net.Mail;
using System.Collections.Concurrent;


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


        using var producer = Task.Run(() => Producer(stock, settings.Notify, queue, settings.Api.Period));
        using var consumer = Task.Run(() => Consumer(settings.Host, queue));

        await Task.WhenAll(producer, consumer);
    }


    private static void Producer(Stock.Info stock, Email.Notify notify, BlockingCollection<MailMessage> queue, uint period)
    {
        while (true)
        {
            foreach (var receiver in notify.Receivers)
            {

                using var message = Email.Message.Buy(notify.Sender, receiver, stock, 1.0M);

                Util.Logger.Log(
                    $"Enqueuing message: From = {message.From}, To = {message.To}, Subject = {message.Subject}"
                );

                queue.Add(message);
                Thread.Sleep((int)period);
            }
        }
    }
    

    private static async void Consumer(Email.Host host, BlockingCollection<MailMessage> queue) {
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

            try
            {
                await client.Send(message);
            }
            catch (Exception exception)
            {
                Util.Error.Exit($"Failed to send email: {exception.Message}", Email.Client.ErrorCode);
            }

            // Avoid passing the SMTP server rate limit
            Thread.Sleep((int)host.Period);
        }
    }
}
