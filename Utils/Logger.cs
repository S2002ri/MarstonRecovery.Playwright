using System;

namespace MarstonRecovery.Tests;

public static class Logger
{
    public static void Info(string message)
    {
        Console.WriteLine(
            $"[INFO] [{DateTime.Now:HH:mm:ss}] {message}");
    }

    public static void Error(string message)
    {
        Console.WriteLine(
            $"[ERROR] [{DateTime.Now:HH:mm:ss}] {message}");
    }
}