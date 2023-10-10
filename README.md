# Stock Quote Alert: Take-home challenge from Inoa

The assignment is to build a C# command-line application that, given a stock ticker symbol and reference prices for selling and buying, keeps checking the stock price and sends emails to an account, specified on a settings file, every time the price goes over or under the references.


## Build and execute

This repo contains the Visual Studio solution files, so the easiest way to run this project is to open it in Visual Studio and execute it from there. The application requires 3 postional arguments: the stock symbol, the reference selling price and the reference buying price.

```~shell
StockQuoteAlert.exe PETR4 22,67 22,59
```

The [properties file](Properties/launchSettings.json) defines the arguments in the example above as the inputs for the program, but this can be configured on the IDE.

### Settings file

On top of the CLI arguments, this app uses a settings file called [`appsettings.json`](appsettings.json) to define its execution parameters. The file version uploaded to this repo doesn't have valid credentials and **must be completed before execution**.

- **Host:** specifies the host address and port for the SMPT server used to send the emails, the username and password credentials to access the server and the period in milliseconds that the program will wait before sending another email to avoid spamming and passing the server rate limit.
- **Notify:** has an email address and display name pair to identify the account responsible for sending the emails and a list of similar pairs to determine the accounts who will receive the recommended action emails.
- **Api:** has the access token for the [brapi API](https://brapi.dev/) and the period in milliseconds that the program will wait before querying the API again.

> **Warning:** [`appsettings.json`](appsettings.json) must be in the directory where the app is launched for it to work. The project solution is configured to automatically copy this file to the directory where the executable is built. If, for some reason, this doesn't work, try manually copying the file.


## About this implementation

### Stock API

There wasn't a requirement to use a specific stock API, so the [brapi API](https://brapi.dev/) was chosen because it is very simple and has a free tier for testing.

### General architecture

The overwall program architecture is based on the **producer/consumer pattern**, where the producer generates and enqueues the necessary emails so that the consumer can send them.

- **Main:** parses the CLI arguments and the JSON settings file and launches the parallel tasks.
- **Producer:** queries the stock API and, if necessary, adds the recommended action message to the queue.
- **Consumer:** waits for a message in the queue and sends it to the SMTP server.

### Project structure

- **Program.cs:** the main execution flow separeted into 3 functions: `Main()`, `Producer()` and `Consumer()`.
- **Stock.cs:** definition for a stock info `struct` and a parser for prices represented in `decimal`.
- **Util.cs:** simple utilities like a global logger and an error `Exit()` function.
- **Cli.cs:** parser for the CLI arguments and definition of the `--help` message.
- **Settings.cs:** parser for the JSON settings file.
- **Email.cs:** abstraction over the SMTP client and builders for the recommended action messages.
- **Api.cs:** abstraction over the HTTP client that communicates with the stock API.

### Coupling

By far the most coupled piece of software in this project is the CLI parser, since it is tuned specifically for the needs of this application. But developing a fully generic parser based on reflection was out of scope for this challenge.

The whole logic of what to do after detecting a price over or under the given references is also embedded in the `Producer()` function. This route was taken due to the simplicity of the actions performed. If, however, this logic was more complicated, an option would be to apply the **publisher/subscriber pattern** to separate the concerns of detecting the events and reacting to them.

### Test enviroment

During development, the free tier of the [Mailtrap](https://mailtrap.io/) SMTP service was used to verify that the emails were being sent correctly.
