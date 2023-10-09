namespace Util;

using System.Diagnostics.CodeAnalysis;


internal static class Error
{
    [DoesNotReturn]
    public static void Exit(string message, int code)
    {
        Console.WriteLine($"Error: {message}");
        Environment.Exit(code);
    }
}


internal static class Logger
{
    public static void Log(string message) => Console.WriteLine($"{DateTime.Now} {message}\n");
}
