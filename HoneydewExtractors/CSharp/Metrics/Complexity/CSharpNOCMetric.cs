using HoneydewExtractors.Core.Metrics;
using HoneydewModels;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Complexity
{
    // NOC = Number of Classes
    public class CSharpNocMetric : IComplexityMetric<RepositoryModel>
    {
        private int _numberOfClasses;

        public void Calculate(RepositoryModel model)
        {
            foreach (var solutionModel in model.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        _numberOfClasses += namespaceModel.ClassModels.Count;
                    }
                }
            }
        }

        public IMetricValue GetMetric()
        {
            return new MetricValue<int>(_numberOfClasses);
        }

        public string PrettyPrint()
        {
            return "Number of Classes";
        }
    }
}
