namespace Honeydew.Logging;

public interface IProgressLoggerBar
{
    void Start();

    void Step(string text);

    void Stop();
}
