namespace HoneydewCore.Logging
{
    public interface IProgressLoggerBar
    {
        void Start();
        void Step(string text);
        void Stop();
    }
}
