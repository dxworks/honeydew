using HoneydewModels;

namespace HoneydewExtractors.Core.Metrics.Extraction
{
    public interface ILinesOfCodeCounter
    {
        LinesOfCode Count(string fileContent);
    }
}
