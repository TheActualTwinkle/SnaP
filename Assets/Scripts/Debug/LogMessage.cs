using System;
using System.Globalization;

namespace PokerLogs
{
    public readonly struct LogMessage
    {
        public DateTime Time { get; }
        public Logger.LogLevel LogLevel { get; }
        public object Message { get; }

        public LogMessage(DateTime dateTime, object message, Logger.LogLevel logLevel)
        {
            Time = dateTime;
            Message = message;
            LogLevel = logLevel;
        }

        public static bool TryParse(string data, out LogMessage logMessage)
        {
            string dateTimeString = data.Substring("[", "]", StringComparison.CurrentCulture);
            if (DateTime.TryParse(dateTimeString, CultureInfo.CurrentCulture, DateTimeStyles.None , out DateTime dateTime) == false)
            {
                logMessage = new LogMessage();
                return false;
            }

            string logLevelString = data.Substring("] ", ":", StringComparison.CurrentCulture);
            if (Enum.TryParse(logLevelString, out Logger.LogLevel logLevel) == false)
            {
                logMessage = new LogMessage();
                return false;
            }

            string message = data.Remove(0, data.LastIndexOf(logLevelString, StringComparison.Ordinal) + logLevelString.Length + 2); // 2 is ':' and ' '.

            logMessage = new LogMessage(dateTime, message, logLevel);
            return true;
        }
        
        public override string ToString()
        {
            return $"[{Time}] {LogLevel}: {Message}";
        }
    }   
}