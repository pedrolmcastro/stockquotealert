using System.Net.Mail;
using System.Collections.Concurrent;


internal static class Program
{
    public static void Main(string[] args)
    {
        var parameters = Cli.Parser.Parse(args);
        Logging.Logger.Log($"Parsed parameters: {parameters}");

        var settings = Settings.Parser.Parse("appsettings.json");
        Logging.Logger.Log($"Parsed settings: {settings}");


        // Start the Consumer Task to consume the queue of emails

        using var emails = new BlockingCollection<MailMessage>();

        Console.CancelKeyPress += delegate
        {
            emails.CompleteAdding();
        };

        using var consumer = Task.Run(
            () => EmailConsumer(settings.Smtp.Host, settings.Smtp.Credential, emails, settings.Smtp.Period)
        );


        // The Main Task produces emails to send

        while (true)
        {
            foreach (var receiver in settings.Smtp.Receivers)
            {
                using var message = Email.Message.Buy(
                    settings.Smtp.Sender,
                    receiver,
                    parameters.Asset,
                    1.0M,
                    parameters.BuyPrice
                );

                Logging.Logger.Log(
                    $"Enqueuing message: From = {message.From}, To = {message.To}, Subject = {message.Subject}"
                );

                emails.Add(message);
                Thread.Sleep((int)settings.Smtp.Period);
            }
        }
    }
    

    private static void EmailConsumer(
        Email.Host host,
        Email.Credential credential,
        BlockingCollection<MailMessage> queue,
        uint period
    ) {
        using var client = new Email.Client(host, credential);

        while (true)
        {
            MailMessage message;

            try
            {
                message = queue.Take();
            }
            catch (InvalidOperationException)
            {
                // The queue concluded adding and is empty
                return;
            }

            Logging.Logger.Log(
                $"Sending message: From = {message.From}, To = {message.To}, Subject = {message.Subject}"
            );

            client.Send(message);

            // Avoid passing the SMTP server rate limit
            Thread.Sleep((int)period);
        }
    }
}
