using UnityEngine;

public class Logger
{
    internal static bool LoggingEnabled = true;

    private static string Sender;
    // internal enum LogLevel { Low, Medium, High }

    internal static void SetSender(string sender) => Sender = $"[{sender}] ";

    internal static void Log(string msg) {
        if(LoggingEnabled) Debug.Log(Sender + msg);
    }
}
