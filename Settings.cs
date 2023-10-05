namespace Settings;


public readonly record struct Parsed
(
    string Host,
    ushort Port,
    List<string> Notified
);


public static class Parser
{
    private const int ErrorCode = 2;


    public static Parsed Parse(string filepath)
    {
        if (!System.IO.File.Exists(filepath))
        {
            ErrorExit($"Missing settings file: {filepath}");
        }

        var parsed = new Parsed();
        
        try
        {
            var content = System.IO.File.ReadAllText(filepath);
            parsed = System.Text.Json.JsonSerializer.Deserialize<Parsed>(content);
        }
        catch
        {
            ErrorExit($"Failed to parse settings file: {filepath}");
        }

        return parsed;
    }


    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void ErrorExit(string message)
    {
        Console.WriteLine($"Error: {message}");
        Environment.Exit(ErrorCode);
    }
}
