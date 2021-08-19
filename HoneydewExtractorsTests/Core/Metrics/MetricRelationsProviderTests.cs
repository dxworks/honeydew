using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.Core.Metrics
{
    public class MetricRelationsProviderTests
    {
        private readonly MetricRelationsProvider _sut;

        public MetricRelationsProviderTests()
        {
            _sut = new MetricRelationsProvider();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData("SomeExtractor")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.InvalidMetric0")]
        public void
            GetFileRelations_ShouldReturnEmptyList_WhenProvidedWithClassMetricWithInvalidExtractorName(
                string extractorName)
        {
            var classMetric = new MetricModel
            {
                ExtractorName = extractorName
            };

            Assert.Empty(_sut.GetFileRelations(classMetric));
        }

        [Theory]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpBaseClassMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit.CSharpUsingsCountMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpIsAbstractMetric")]
        public void
            GetFileRelations_ShouldReturnEmptyList_WhenProvidedWithClassMetricWithExtractorNameOfMetricsThatAreNotRelationMetrics(
                string extractorName)
        {
            var classMetric = new MetricModel
            {
                ExtractorName = extractorName
            };

            Assert.Empty(_sut.GetFileRelations(classMetric));
        }

        [Theory]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpReturnValueRelationMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpLocalVariablesRelationMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpParameterRelationMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpPropertiesRelationMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpFieldsRelationMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpObjectCreationRelationMetric")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric.CSharpExceptionsThrownRelationMetric")]
        public void GetFileRelations_ShouldReturnListWithRelations_WhenProvidedClassMetricWithRelationMetrics(
            string extractorName)
        {
            var classMetric = new MetricModel
            {
                ExtractorName = extractorName,
                ValueType = "HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpRelationDataMetric",
                Value = new Dictionary<string, int>
                {
                    {"Class1", 2},
                    {"Class2", 12},
                    {"Class3", 5},
                }
            };

            var fileRelations = _sut.GetFileRelations(classMetric);
            Assert.Equal(3, fileRelations.Count);

            var fileRelation0 = fileRelations[0];
            Assert.Equal(extractorName, fileRelation0.RelationType);
            Assert.Equal(2, fileRelation0.RelationCount);
            Assert.Equal("Class1", fileRelation0.FileTarget);

            var fileRelation1 = fileRelations[1];
            Assert.Equal(extractorName, fileRelation1.RelationType);
            Assert.Equal(12, fileRelation1.RelationCount);
            Assert.Equal("Class2", fileRelation1.FileTarget);

            var fileRelation2 = fileRelations[2];
            Assert.Equal(extractorName, fileRelation2.RelationType);
            Assert.Equal(5, fileRelation2.RelationCount);
            Assert.Equal("Class3", fileRelation2.FileTarget);
        }
    }
}
