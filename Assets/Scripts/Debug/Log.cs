using System;
using System.IO;
using UnityEngine;

public static class Log
{
    private static DateTime DateTime => DateTime.Now;
    private static RuntimePlatform Platform => Application.platform;

    private static readonly string LOGFilePath = $"{Application.persistentDataPath}\\CustomLog.log";
    
    public static void WriteToFile(object message)
    {
        using StreamWriter sw = new(LOGFilePath, true);
        sw.WriteLine($"[{DateTime}] {message} Platform: {Platform}");

        #if UNITY_EDITOR
        WriteLine(message);
        #endif
    }    
    
    private static void WriteLine(object message)
    {
        Debug.Log($"[{DateTime}] {message} Platform: {Platform}");
    }
}
