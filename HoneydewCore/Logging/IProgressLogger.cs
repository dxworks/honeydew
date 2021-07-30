namespace HoneydewCore.Logging
{
    public interface IProgressLogger
    {
        public void Log(string value = "", LogLevels logLevel = LogLevels.Information);
    }

    public enum LogLevels
    {
        Information,
        Warning,
        Error,
    }
}
