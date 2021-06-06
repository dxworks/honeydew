using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SyntacticMetrics
{
    public class UsingsCountMetricTests
    {
        private readonly CSharpMetricExtractor _sut;
        private Extractor<CSharpMetricExtractor> _extractor;

        public UsingsCountMetricTests()
        {
            _sut = new UsingsCountMetric();
        }

        [Fact]
        public void Name_ShouldReturn_UsingsCount()
        {
            Assert.Equal("Usings Count", _sut.GetName());
        }

        [Fact]
        public void GetMetricType_ShouldReturnSyntactic()
        {
            Assert.Equal(MetricType.Syntactic, _sut.GetMetricType());
        }
        
        [Fact]
        public void Extract_ShouldHaveUsingsCountMetric_WhenGivenMultipleUsingsAtMultipleLevels()
        {
            const string fileContent = @"using System;
                                    using System.Collections.Generic;
                                    using System.Linq;
                                    using System.Text;
                                    using Microsoft.CodeAnalysis;
                                    using Microsoft.CodeAnalysis.CSharp;

                                    namespace TopLevel
                                    {
                                        using Microsoft;
                                        using System.ComponentModel;

                                        namespace Child1
                                        {
                                            using Microsoft.Win32;
                                            using System.Runtime.InteropServices;

                                            public class Foo { }
                                        }

                                        namespace Child2
                                        {
                                            using System.CodeDom;
                                            using Microsoft.CSharp;

                                            public class Bar { public void b(){} }
                                        }
                                    }";

            
            var metrics = new List<CSharpMetricExtractor>()
            {
                _sut
            };

            _extractor = new CSharpClassExtractor(metrics);

            var compilationUnitModel = _extractor.Extract(fileContent);

            Assert.Equal(1, compilationUnitModel.Metrics.Count);
            
            var metric = compilationUnitModel.Metrics[_sut.GetName()];
            Assert.Equal(typeof(Metric<int>), metric.GetType());


            var count = ((Metric<int>) metric).Value;

            Assert.Equal(12, count);
        }
    }
}