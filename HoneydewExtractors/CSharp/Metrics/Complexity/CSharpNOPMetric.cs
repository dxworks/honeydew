using HoneydewExtractors.Core.Metrics;
using HoneydewModels;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Complexity
{
    // NOP = Number of packages (namespaces)
    public class CSharpNopMetric : IComplexityMetric<RepositoryModel>
    {
        private int _numberOfPackages;

        public void Calculate(RepositoryModel model)
        {
            foreach (var solutionModel in model.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    _numberOfPackages += projectModel.Namespaces.Count;
                }
            }
        }

        public IMetricValue GetMetric()
        {
            return new MetricValue<int>(_numberOfPackages);
        }

        public string PrettyPrint()
        {
            return "Number of Packages";
        }
    }
}
