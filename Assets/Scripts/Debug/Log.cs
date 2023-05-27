// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
// ReSharper disable UnusedMember.Local

public static class Log
{
    private static readonly string LogFilePath = $"{Application.persistentDataPath}\\CustomLog.log";

    private static DateTime DateTime => DateTime.Now;
    private static RuntimePlatform Platform => Application.platform;

    static Log()
    {
        if (File.Exists(LogFilePath) == false)
        {
            File.Create(LogFilePath).Close();
        }
        else
        {
            File.WriteAllText(LogFilePath, string.Empty);
        }
    }
    
    public static void WriteToFile(object message)
    {
        Debug.Log(message);
        
#if !UNITY_EDITOR

        try
        {
            using StreamWriter sw = new(LogFilePath, true);

            string ip = "";//IPAddressPresenter.Address.Replace("\n", string.Empty).Replace("\r", string.Empty);
            message = $"[{DateTime}] {message} Platform: {Platform}. IP: {ip}";
            sw.WriteLine(message);
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(e)} {e.Message}");
        }
        
#endif
    }
}
