namespace HoneydewCore.Logging
{
    public interface IProgressLogger
    {
        void Start();
        void Step(string text);
        void Stop();
    }
}
