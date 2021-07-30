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

        public string GetHistory()
        {
            return _history.ToString();
        }

        public void Log(string value = "", LogLevels logLevel = LogLevels.Information)
        {
            _progressLogger.Log(value);
            _history.AppendLine(value);
        }
    }
}
