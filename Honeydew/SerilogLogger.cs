using HoneydewCore.Logging;
using Serilog;
using Serilog.Core;
using ILogger = HoneydewCore.Logging.ILogger;

namespace Honeydew
{
    public class SerilogLogger : ILogger
    {
        private readonly Logger _logger;

        public SerilogLogger(string filePath = "")
        {
            var loggerConfiguration = new LoggerConfiguration();
                // .WriteTo.Console();
            if (!string.IsNullOrEmpty(filePath))
            {
                loggerConfiguration = loggerConfiguration.WriteTo.File(filePath);
            }

            _logger = loggerConfiguration
                .CreateLogger();
        }

        public void Log(string value = "", LogLevels logLevel = LogLevels.Information)
        {
            switch (logLevel)
            {
                default:
                // case LogLevels.Information:
                    _logger.Information(value);
                    break;
                case LogLevels.Warning:
                    _logger.Warning(value);
                    break;
                case LogLevels.Error:
                    _logger.Error(value);

                    break;
            }
        }

        public void CloseAndFlush()
        {
            _logger.Dispose();
        }
    }
}
