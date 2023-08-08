// ReSharper disable RedundantUsingDirective
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
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
    
    public static void Log(object message, Level level)
    {
#if !UNITY_EDITOR

        WriteToFile(message, level);
        
#endif
        Debug.Log(message);
    }

    private static void WriteToFile(object message, Level level)
    {
        try
        {
            using StreamWriter sw = new(PokerLogReaderFilePath, true);
            sw.WriteLine(new LogMessage(DateTime, message, level).ToString());
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
