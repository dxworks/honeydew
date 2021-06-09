using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Readers.Strategies;
using Xunit;

namespace HoneydewCoreTest.Extractors.Strategies
{
    public class DirectFactExtractionTests
    {
        private readonly DirectSolutionLoading _sut;

        public DirectFactExtractionTests()
        {
            _sut = new DirectSolutionLoading();
        }

        [Fact]
        public void Extract_ShouldReturnEmptyList_WhenGivenEmptyContentAndNullExtractors()
        {
            var compilationUnitModels = _sut.Load("", null);
            Assert.Empty(compilationUnitModels);
        }

        [Fact]
        public void Extract_ShouldReturnEmptyList_WhenGivenEmptyContentAndNoExtractors()
        {
            var compilationUnitModels = _sut.Load("\t ", new List<IFactExtractor>());
            Assert.Empty(compilationUnitModels);
        }

        [Fact]
        public void Extract_ShouldReturnEmptyList_WhenGivenSomeContentContentAndNoExtractors()
        {
            const string fileContent = @"namespace Models
                                    {
                                      public class Item { }
                                    }";
            var compilationUnitModels = _sut.Load(fileContent, new List<IFactExtractor>());
            Assert.Empty(compilationUnitModels);
        }

        [Fact]
        public void
            Extract_ShouldReturnListWithOneItemWithBasicInfo_WhenGivenSomeContentAndOneExtractorWithNoMetricExtractors()
        {
            const string fileContent = @"namespace Models
                                    {
                                      public class Item { }
                                    }";
            var extractors = new List<IFactExtractor>
            {
                new CSharpClassFactExtractor(new List<CSharpMetricExtractor>())
            };
            var compilationUnitModels = _sut.Load(fileContent, extractors);

            Assert.Equal(1, compilationUnitModels.Count);
            var compilationUnitModel = compilationUnitModels[0];

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(1, compilationUnitModel.ClassModels.Count);

            var classModel = compilationUnitModel.ClassModels[0];
            Assert.Equal("Models", classModel.Namespace);
            Assert.Equal("Item", classModel.Name);
            Assert.False(classModel.Metrics.HasMetrics());
        }

        [Fact]
        public void
            Extract_ShouldReturnListWithOneItemWithBasicInfoAndUsingsCount_WhenGivenContentAndOneExtractorWithUsingsCountMetricExtractors()
        {
            const string fileContent = @"
                                    using System;
                                    using HoneydewCore.Extractors.Metrics;
                                    using HoneydewCore.Extractors.Strategies;
                                    using Xunit;

                                    namespace Models
                                    {
                                      public class Item { }
                                    }";
            var extractors = new List<IFactExtractor>
            {
                new CSharpClassFactExtractor(new List<CSharpMetricExtractor>
                {
                    new UsingsCountMetric()
                })
            };
            var compilationUnitModels = _sut.Load(fileContent, extractors);

            Assert.Equal(1, compilationUnitModels.Count);
            var compilationUnitModel = compilationUnitModels[0];

            Assert.True(compilationUnitModel.SyntacticMetrics.HasMetrics());
            var optional = compilationUnitModel.SyntacticMetrics.Get<UsingsCountMetric>();
            Assert.True(optional.HasValue);
            var metric = (Metric<int>) optional.Value;
            Assert.Equal(4, metric.Value);

            Assert.Equal(1, compilationUnitModel.ClassModels.Count);

            var classModel = compilationUnitModel.ClassModels[0];
            Assert.Equal("Models", classModel.Namespace);
            Assert.Equal("Item", classModel.Name);
            Assert.False(classModel.Metrics.HasMetrics());
        }
    }
}