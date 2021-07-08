using System.Text;
using HoneydewCore.Logging;

namespace Honeydew
{
    public class ConsoleLoggerWithHistory : IProgressLogger
    {
        private readonly IProgressLogger _progressLogger;
        private readonly StringBuilder _history = new();

        public ConsoleLoggerWithHistory(IProgressLogger progressLogger)
        {
            _progressLogger = progressLogger;
        }

        public void LogLine(string value = "")
        {
            _progressLogger.LogLine(value);
            _history.AppendLine(value);
        }

        public void Log(string value)
        {
            _progressLogger.Log(value);
            _history.Append(value);
        }

        public string GetHistory()
        {
            return _history.ToString();
        }
    }
}