using HoneydewExtractors.Core.Metrics.Extraction;

namespace HoneydewExtractors.Core
{
    public interface IFactExtractorCreator
    {
        IFactExtractor Create(string language);
    }
}
