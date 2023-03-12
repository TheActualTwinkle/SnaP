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

    private static bool _appendLogFile;
    
    public static void WriteToFile(object message)
    {
        Debug.Log(message);

#if !UNITY_EDITOR

        try
        {
            if (File.Exists(LogFilePath) == false)
            {
                File.Create(LogFilePath);
            }
        
            using StreamWriter sw = new(LogFilePath, _appendLogFile);
            _appendLogFile = true;

            message = $"[{DateTime}] {message} Platform: {Platform}. IP: {GetIp()}";
            sw.WriteLine(message);
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(e)} {e.Message}");
        }
        

#endif
    }

    private static string GetIp()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
    }
}
