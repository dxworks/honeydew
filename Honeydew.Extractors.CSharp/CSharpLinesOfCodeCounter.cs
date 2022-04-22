namespace Honeydew.Extractors.CSharp;

public class CSharpLinesOfCodeCounter : LinesOfCodeCounter
{
    public CSharpLinesOfCodeCounter() : base("//", "/*", "*/")
    {
    }
}
