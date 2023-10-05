internal static class Program
{
    public static void Main(string[] args)
    {
        var parameters = Cli.Parser.Parse(args);
        Console.WriteLine(parameters);
    }
}
