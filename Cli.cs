// System.CommandLine is still in beta and doesn't support positional parameters
// This application specific utility was built as a replacement

namespace Cli;


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
        get { return "--" + _long; }
    }

    public string Short
    {
        get { return "-" + _short; }
    }

    public override string ToString()
    {
        return $"{Long}, {Short}: {Description}";
    }


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
    public override string ToString()
    {
        return $"{Name}: {Description}";
    }
}


internal readonly record struct Parsed(string Asset, decimal SellPrice, decimal BuyPrice);


internal static class Parser
{
    private const int ErrorCode = 1;
    private const string Description = "Asset stock price monitor";

    private readonly static string s_executable = Environment.GetCommandLineArgs()[0];
    private readonly static Option s_help = new("help", "Show help and usage information");

    private readonly static Parameter[] s_parameters = new Parameter[]
    {
        new Parameter("Asset", "Name of the market asset to monitor"),
        new Parameter("SellPrice", "Selling reference price that notifies if price is greater"),
        new Parameter("BuyPrice", "Buying reference price that notifies if price is lower")
    };

    private readonly static string s_usage =
        $"{s_executable} [{s_help.Short}] {string.Join(" ", s_parameters.Select(param => param.Name))}";


    public static Parsed Parse(IList<string> args)
    {
        if (args.Any(s_help.Match))
        {
            HelpExit();
        }

        if (args.Count < s_parameters.Length)
        {
            ErrorExit("Missing positional parameters");
        }

        if (!Price.Parser.TryParse(args[1], out var sellPrice))
        {
            ErrorExit($"Invalid selling reference price: {args[1]}");
        }

        if (!Price.Parser.TryParse(args[2], out var buyPrice))
        {
            ErrorExit($"Invalid buying reference price: {args[2]}");
        }

        return new Parsed { Asset = args[0], SellPrice = sellPrice, BuyPrice = buyPrice };
    }


    private static string FormatItems<T>(string group, IEnumerable<T> items)
    {
        return $"{group}\n  {string.Join("\n  ", items)}\n";
    }

    private static string FormatItem<T>(string group, T item)
    {
        return FormatItems(group, Enumerable.Repeat(item, 1));
    }


    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void HelpExit()
    {
        var description = FormatItem("Description", Description);
        var parameters = FormatItems("Parameters", s_parameters);
        var options = FormatItem("Options", s_help);
        var usage = FormatItem("Usage", s_usage);

        Console.WriteLine($"{description}\n{usage}\n{parameters}\n{options}");
        Environment.Exit(0);
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void ErrorExit(string message)
    {
        Console.WriteLine($"{FormatItem("Error", message)}\n{FormatItem("Usage", s_usage)}");
        Environment.Exit(ErrorCode);
    }
}
