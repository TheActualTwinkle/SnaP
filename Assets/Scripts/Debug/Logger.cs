using System;
using System.IO;
using PokerLogs;
using UnityEngine;
using Application = UnityEngine.Application;

// ReSharper disable UnusedMember.Local

public static class Logger
{
    private static readonly string PokerLogReaderFilePath = $"{Application.persistentDataPath}\\Log.plr";

    private static DateTime DateTime => DateTime.Now;
    private static RuntimePlatform Platform => Application.platform;

    static Logger()
    {
        if (File.Exists(PokerLogReaderFilePath) == false)
        {
            File.Create(PokerLogReaderFilePath).Close();
        }
        else
        {
            File.WriteAllText(PokerLogReaderFilePath, $"App Version: {Application.version}. Runtime platform: {Platform.ToString()}.\n\r");
        }
    }
    
    public static void Log(object message, Level level = Level.Info)
    {
        LogMessage logMessage = new(DateTime, message, level);
        
#if !UNITY_EDITOR

        WriteToFile(logMessage, level);
        
#endif

        switch (level)
        {
            case Level.Info:
                Debug.Log(logMessage);
                break;
            case Level.Warning:
                Debug.LogWarning(logMessage);
                break;
            case Level.Error:
                Debug.LogError(logMessage);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    private static void WriteToFile(LogMessage message, Level level)
    {
        try
        {
            using StreamWriter sw = new(PokerLogReaderFilePath, true);
            sw.WriteLine(message.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(e)} {e.Message}");
        }
    }

    public enum Level
    {
        Info,
        Warning,
        Error,
    }
}
