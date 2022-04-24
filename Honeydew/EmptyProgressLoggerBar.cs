using Honeydew.Logging;

namespace Honeydew;

public class EmptyProgressLoggerBar : IProgressLoggerBar
{
    private readonly string _text;

    public EmptyProgressLoggerBar(string text)
    {
        _text = text;
    }

    public void Start()
    {
        Console.WriteLine($"Start {_text}");
    }

    public void Step(string text)
    {
        Console.WriteLine(text);
    }

    public void Stop()
    {
        Console.WriteLine($"Done {_text}");
    }
}
