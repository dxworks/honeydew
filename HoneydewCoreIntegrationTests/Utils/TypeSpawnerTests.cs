using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
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

            extractionMetricSpawner.LoadType<CSharpBaseClassMetric>();
            extractionMetricSpawner.LoadType<CSharpUsingsCountMetric>();
            extractionMetricSpawner.LoadType<CSharpBaseClassMetric>();
            extractionMetricSpawner.LoadType<CSharpUsingsCountMetric>();
            extractionMetricSpawner.LoadType<CSharpBaseClassMetric>();


            var instantiateMetrics = extractionMetricSpawner.InstantiateMetrics();

            Assert.Equal(2, instantiateMetrics.Count);

            Assert.Equal(typeof(CSharpBaseClassMetric),instantiateMetrics[0].GetType());
            Assert.Equal(typeof(CSharpUsingsCountMetric),instantiateMetrics[1].GetType());
        }
        
        [Fact]
        public void Load_ShouldLoadMetricsOnce_WhenProvidedWithDifferentMetrics()
        {
            TypeSpawner<IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>>
                extractionMetricSpawner = new();

            extractionMetricSpawner.LoadType<CSharpBaseClassMetric>();
            extractionMetricSpawner.LoadType<CSharpIsAbstractMetric>();
            extractionMetricSpawner.LoadType<CSharpFieldsInfoMetric>();
            extractionMetricSpawner.LoadType<CSharpUsingsCountMetric>();
            extractionMetricSpawner.LoadType<CSharpMethodInfoMetric>();


            var instantiateMetrics = extractionMetricSpawner.InstantiateMetrics();

            Assert.Equal(5, instantiateMetrics.Count);

            Assert.Equal(typeof(CSharpBaseClassMetric),instantiateMetrics[0].GetType());
            Assert.Equal(typeof(CSharpIsAbstractMetric),instantiateMetrics[1].GetType());
            Assert.Equal(typeof(CSharpFieldsInfoMetric),instantiateMetrics[2].GetType());
            Assert.Equal(typeof(CSharpUsingsCountMetric),instantiateMetrics[3].GetType());
            Assert.Equal(typeof(CSharpMethodInfoMetric),instantiateMetrics[4].GetType());
        }
    }
}
