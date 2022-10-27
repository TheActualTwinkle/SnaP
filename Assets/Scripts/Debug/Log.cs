using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class Log
{
    private static DateTime _dateTime => DateTime.Now;
    private static RuntimePlatform _platform => Application.platform;

    public static void WriteLine(object message)
    {
        Debug.Log($"[{_dateTime}] {message} Platform: {_platform}");
    }
}
