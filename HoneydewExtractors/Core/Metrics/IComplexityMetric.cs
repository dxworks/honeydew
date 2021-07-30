using HoneydewModels;

namespace HoneydewExtractors.Core.Metrics
{
    public interface IComplexityMetric<in TRepositoryModel> : IMetric
        where TRepositoryModel : IRepositoryModel
    {
        void Calculate(TRepositoryModel model);
    }
}
