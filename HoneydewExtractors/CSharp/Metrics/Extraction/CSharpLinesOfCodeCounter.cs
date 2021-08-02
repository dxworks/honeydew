using HoneydewExtractors.Core.Metrics.Extraction;

namespace HoneydewExtractors.CSharp.Metrics.Extraction
{
    public class CSharpLinesOfCodeCounter : LinesOfCodeCounter
    {
        public CSharpLinesOfCodeCounter() : base("//", "/*", "*/")
        {
        }
    }
}
