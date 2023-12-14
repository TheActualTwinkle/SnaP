using System;
using System.Globalization;

namespace PokerLogs
{
    public readonly struct LogMessage
    {
        public DateTime Time { get; }
        public Logger.LogLevel LogLevel { get; }
        public object Message { get; }
        public Logger.LogSource LogSource { get; }

        public LogMessage(DateTime dateTime, object message, Logger.LogLevel logLevel, Logger.LogSource logSource)
        {
            Time = dateTime;
            Message = message;
            LogLevel = logLevel;
            LogSource = logSource;
        }

        public static bool TryParse(string data, out LogMessage logMessage)
        {
            string logSourceString = data.Substring("[", "]", StringComparison.CurrentCulture);
            if (Enum.TryParse(logSourceString, out Logger.LogSource logSource) == false)
            {
                logMessage = new LogMessage();
                return false;
            }

            data = data.Remove(0, data.LastIndexOf(logSourceString, StringComparison.Ordinal) + 1); // +1 is ' '.
            
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

            string message = data.Remove(0, data.LastIndexOf(logLevelString, StringComparison.Ordinal) + logLevelString.Length + 2); // +2 is ':' and ' '.

            logMessage = new LogMessage(dateTime, message, logLevel, logSource);
            return true;
        }
        
        public override string ToString()
        {
            return $"[{LogSource}] [{Time}] {LogLevel}: {Message}";
        }
    }   
}