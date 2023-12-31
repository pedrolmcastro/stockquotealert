﻿// System.CommandLine is still in beta and doesn't support positional parameters
// This application specific utility was built as a replacement

namespace Cli;

using System.Diagnostics.CodeAnalysis;


internal readonly struct Option
{
    private readonly string _long;
    private readonly string _short;
    public readonly string Description;


    public Option(string @short, string @long, string description = "")
    {
        _long = @long;
        _short = @short;
        Description = description;
    }

    public Option(string @long, string description = "")
        : this(@long[..1], @long, description)
    {}
    
    
    public string Long
    {
        get => $"--{_long}";
    }

    public string Short
    {
        get => $"-{_short}";
    }

    public override string ToString() => $"{Long}, {Short}: {Description}";


    public bool Match(string matched)
    {
        if (matched.StartsWith("--"))
        {
            return _long == matched[2..];
        }
        
        if (matched.StartsWith('-'))
        {
            return _short == matched[1..];
        }

        return false;
    }
}


internal readonly record struct Parameter(string Name, string Description)
{
    public override string ToString() => $"{Name}: {Description}";
}


internal static class Parser
{
    private const int ErrorCode = 1;
    private const string Description = "Stock price monitor";

    private readonly static string s_executable = Environment.GetCommandLineArgs()[0];
    private readonly static Option s_help = new("help", "Show help and usage information");

    private readonly static Parameter[] s_parameters = new Parameter[]
    {
        new("StockTicker", "Ticker symbol of the stock to be monitored"),
        new("SellPrice", "Selling reference price that notifies if the price is greater than it"),
        new("BuyPrice", "Buying reference price that notifies if the price is lower than it")
    };

    private readonly static string s_usage =
        $"{s_executable} [{s_help.Short}] {string.Join(" ", s_parameters.Select(param => param.Name))}";


    public static Stock.Info Parse(IList<string> args)
    {
        if (args.Any(s_help.Match))
        {
            Help();
        }

        if (args.Count < s_parameters.Length)
        {
            Error("Missing positional parameters");
        }

        if (!Stock.Price.TryParse(args[1], out var sellPrice))
        {
            Error($"Invalid selling reference price: {args[1]}");
        }

        if (!Stock.Price.TryParse(args[2], out var buyPrice))
        {
            Error($"Invalid buying reference price: {args[2]}");
        }

        return new() { Ticker = args[0], SellPrice = sellPrice, BuyPrice = buyPrice };
    }


    private static string FormatItems<T>(string group, IEnumerable<T> items)
    {
        return $"{group}\n  {string.Join("\n  ", items)}\n";
    }

    private static string FormatItem<T>(string group, T item)
    {
        return FormatItems(group, Enumerable.Repeat(item, 1));
    }


    [DoesNotReturn]
    private static void Help()
    {
        var description = FormatItem("Description", Description);
        var parameters = FormatItems("Parameters", s_parameters);
        var options = FormatItem("Options", s_help);
        var usage = FormatItem("Usage", s_usage);

        Console.WriteLine($"{description}\n{usage}\n{parameters}\n{options}");
        Environment.Exit(0);
    }

    [DoesNotReturn]
    private static void Error(string message)
    {
        Util.Error.Exit($"{message}\nUsage: {s_usage}", ErrorCode);
    }
}
