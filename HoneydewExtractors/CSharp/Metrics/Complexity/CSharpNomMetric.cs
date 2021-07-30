using HoneydewExtractors.Core.Metrics;
using HoneydewModels;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Complexity
{
    // Number of Operations
    public class CSharpNomMetric : IComplexityMetric<RepositoryModel>
    {
        private int _numberOfMethods;

        public void Calculate(RepositoryModel model)
        {
            foreach (var solutionModel in model.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            _numberOfMethods += classModel.Methods.Count;
                        }
                    }
                }
            }
        }

        public IMetricValue GetMetric()
        {
            return new MetricValue<int>(_numberOfMethods);
        }

        public string PrettyPrint()
        {
            return "Number of Operations";
        }
    }
}
