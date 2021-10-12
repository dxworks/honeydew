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
            foreach (var projectModel in model.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        if (classType is not ClassModel classModel)
                        {
                            continue;
                        }

                        _numberOfMethods += classModel.Methods.Count;
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
