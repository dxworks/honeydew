using Honeydew.Models;

namespace Honeydew.Extractors.Metrics;

public interface ILinesOfCodeCounter
{
    LinesOfCode Count(string fileContent);
}
