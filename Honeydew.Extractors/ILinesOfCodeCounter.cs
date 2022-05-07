using Honeydew.Models;

namespace Honeydew.Extractors;

public interface ILinesOfCodeCounter
{
    LinesOfCode Count(string fileContent);
}
