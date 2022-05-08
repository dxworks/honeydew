using Honeydew.Logging;

namespace Honeydew.ScriptBeePlugin;

internal class EmptyLogger : ILogger
{
    public void Log(string value = "", LogLevels logLevel = LogLevels.Information)
    {
    }
}
