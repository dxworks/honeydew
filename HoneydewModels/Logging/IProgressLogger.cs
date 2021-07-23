namespace HoneydewCore.Logging
{
    public interface IProgressLogger
    {
        public void LogLine(string value = "");

        public void Log(string value);
    }
}