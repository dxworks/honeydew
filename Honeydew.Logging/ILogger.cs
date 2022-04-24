namespace Honeydew.Logging;

public interface ILogger
{
    public void Log(string value = "", LogLevels logLevel = LogLevels.Information);
}

public enum LogLevels
{
    Information,
    Warning,
    Error,
}
