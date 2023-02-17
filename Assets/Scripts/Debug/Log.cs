using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public static class Log
{
    public static readonly string LogFilePath = $"{Application.persistentDataPath}\\CustomLog.log";

    private static TelegramBot TelegramBot => TelegramBot.Instance;
    private static DateTime DateTime => DateTime.Now;
    private static RuntimePlatform Platform => Application.platform;

    private static bool _appendLogFile;
    
    public static void WriteToFile(object message)
    {
                
#if UNITY_EDITOR
        Debug.Log(message);
#endif
        return; // todo
        using StreamWriter sw = new(LogFilePath, _appendLogFile);
        _appendLogFile = true;

        message = $"[{DateTime}] {message} Platform: {Platform}. IP: {GetIp()}";
        sw.WriteLine(message);

        if (TelegramBot != null)
        {
            TelegramBot.SendMessage(message.ToString());
        }
    }

    private static string GetIp()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
    }
}
