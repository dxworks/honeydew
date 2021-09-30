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
            foreach (var projectModel in model.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    _numberOfClasses += compilationUnitType.ClassTypes.Count;
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
