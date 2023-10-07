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
