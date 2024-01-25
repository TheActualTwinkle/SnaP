using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using PokerLogs;
using UnityEngine;
using Application = UnityEngine.Application;

// ReSharper disable UnusedMember.Local

public static class Logger
{
    public static readonly string PokerLogViewerFilePath;

    private static DateTime DateTime => DateTime.Now;
    private static RuntimePlatform Platform => Application.platform;
    
    private static readonly List<LogSource> IgnoredLogSources = new()
    {
        // LogSource.General,
        // LogSource.SnaPDataTransfer,
        // LogSource.Addressables,
    };
    
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    static Logger()
    {
#if UNITY_EDITOR
        return;
#endif
        
#pragma warning disable CS0162
        
        PokerLogViewerFilePath = $"{Application.persistentDataPath}\\Log_{DateTime.UtcNow.ToString(CultureInfo.CurrentCulture).ReplaceAll(new[] {' ', '.', ':', '\\', '/'}, '_')}.snp";
        
        if (File.Exists(PokerLogViewerFilePath) == false)
        {
            File.Create(PokerLogViewerFilePath).Close();
        }

        File.WriteAllText(PokerLogViewerFilePath, $"App Version: {Application.version}. Runtime platform: {Platform.ToString()}.\n\r");
        
#pragma warning restore CS0162
    }

    public static void Log(object message, LogLevel logLevel = LogLevel.Info, LogSource logSource = LogSource.General)
    {
        if (IgnoredLogSources.Contains(logSource) == true)
        {
            return;
        }
        
        
#if !UNITY_EDITOR
        LogMessage logMessage = new(DateTime, message, logLevel, logSource);
        WriteToFile(logMessage);
#endif

        string logSourceString = logSource == LogSource.General ? string.Empty : $"[{logSource.ToString()}] ";
        string fullMessage = logSourceString + message;
        
        switch (logLevel)
        {
            case LogLevel.Info:
                Debug.Log(fullMessage);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(fullMessage);
                break;
            case LogLevel.Error:
                Debug.LogError(fullMessage);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
    
    public static void Log(object message, LogSource logSource)
    {
        Log(message, LogLevel.Info, logSource);
    }

    private static void WriteToFile(LogMessage message)
    {
        try
        {
            using StreamWriter sw = new(PokerLogViewerFilePath, true);
            sw.WriteLine(message.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(e)} {e.Message}");
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
    }
    
    public enum LogSource
    {
        General,
        SnaPDataTransfer,
        Addressables,
    }
}
