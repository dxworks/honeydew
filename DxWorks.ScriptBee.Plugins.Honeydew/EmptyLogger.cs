using Honeydew.Logging;

namespace DxWorks.ScriptBee.Plugins.Honeydew;

internal class EmptyLogger : ILogger
{
    public void Log(string value = "", LogLevels logLevel = LogLevels.Information)
    {
    }
}
