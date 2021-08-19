using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using Xunit;

namespace HoneydewCoreIntegrationTests.Utils
{
    public class TypeSpawnerTests
    {
        [Fact]
        public void Load_ShouldLoadMetricsOnce_WhenProvidedWithTheSameMetricMultipleTimes()
        {
            TypeSpawner<IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>>
                extractionMetricSpawner = new();

            extractionMetricSpawner.LoadType<CSharpUsingsCountMetric>();
            extractionMetricSpawner.LoadType<CSharpUsingsCountMetric>();


            var instantiateMetrics = extractionMetricSpawner.InstantiateMetrics();

            Assert.Equal(1, instantiateMetrics.Count);
            Assert.Equal(typeof(CSharpUsingsCountMetric),instantiateMetrics[0].GetType());
        }
        
        [Fact]
        public void Load_ShouldLoadMetricsOnce_WhenProvidedWithDifferentMetrics()
        {
            TypeSpawner<IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>>
                extractionMetricSpawner = new();

            extractionMetricSpawner.LoadType<CSharpUsingsCountMetric>();


            var instantiateMetrics = extractionMetricSpawner.InstantiateMetrics();

            Assert.Equal(1, instantiateMetrics.Count);
            
            Assert.Equal(typeof(CSharpUsingsCountMetric),instantiateMetrics[0].GetType());
        }
    }
}
