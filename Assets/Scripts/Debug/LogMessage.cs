using System;

namespace PokerLogs
{
    public readonly struct LogMessage
    {
        private readonly DateTime _dateTime;
        private readonly object _message;
        private readonly Logger.Level _logLevel;

        public LogMessage(DateTime dateTime, object message, Logger.Level logLevel)
        {
            _dateTime = dateTime;
            _message = message;
            _logLevel = logLevel;
        }

        public override string ToString()
        {
            return $"[{_dateTime}] Log level: {_logLevel}. {_message}";
        }
    }   
}