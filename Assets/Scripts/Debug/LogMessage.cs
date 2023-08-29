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

        public LogMessage Parse(string str)
        {
            return new LogMessage(); // todo:
        }
        
        public override string ToString()
        {
            return $"[{_dateTime}] {_logLevel}: {_message}";
        }
    }   
}