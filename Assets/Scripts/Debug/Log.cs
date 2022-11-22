using System;
using System.IO;
using UnityEngine;

public static class Log
{
    private static DateTime DateTime => DateTime.Now;
    private static RuntimePlatform Platform => Application.platform;

    public static void WriteLine(object message)
    {
        Debug.Log($"[{DateTime}] {message} Platform: {Platform}");
    }

    public static void WriteToFile(object message, string path)
    {
        using StreamWriter sw = new(path, true);
        sw.WriteLine($"[{DateTime}] {message} Platform: {Platform}");
    }
}
