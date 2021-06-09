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
            var classModels = _sut.Load(fileContent, extractors);

            Assert.Equal(1, classModels.Count);

            Assert.Equal("Models", classModels[0].Namespace);
            Assert.Equal("Item", classModels[0].Name);
            Assert.False(classModels[0].Metrics.HasMetrics());
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
            var classModels = _sut.Load(fileContent, extractors);

            Assert.Equal(1, classModels.Count);

            Assert.True(classModels[0].Metrics.HasMetrics());
            Assert.Equal(1, classModels[0].Metrics.Count);
            
            var optional = classModels[0].Metrics.Get<UsingsCountMetric>();
            Assert.True(optional.HasValue);
            Assert.Equal(4, (int) optional.Value.GetValue());

            Assert.Equal(1, classModels.Count);

            Assert.Equal("Models", classModels[0].Namespace);
            Assert.Equal("Item", classModels[0].Name);
        }
    }
}