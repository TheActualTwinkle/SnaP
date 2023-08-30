using System;
using System.Globalization;

namespace PokerLogs
{
    public readonly struct LogMessage
    {
        private readonly DateTime _dateTime;
        private readonly object _message;
        private readonly Logger.LogLevel _logLevel;

        public LogMessage(DateTime dateTime, object message, Logger.LogLevel logLevel)
        {
            _dateTime = dateTime;
            _message = message;
            _logLevel = logLevel;
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
            return $"[{_dateTime}] {_logLevel}: {_message}";
        }
    }   
}