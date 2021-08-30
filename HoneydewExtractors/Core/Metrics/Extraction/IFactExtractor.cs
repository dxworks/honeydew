using HoneydewModels.Types;

namespace HoneydewExtractors.Core.Metrics.Extraction
{
    public interface IFactExtractor
    {
        public ICompilationUnitType Extract(string fileContent);
    }
}
