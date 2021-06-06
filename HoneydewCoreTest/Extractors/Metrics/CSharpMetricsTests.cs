using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics
{
    public class CSharpMetricsTests // tests with multiple metrics
    {
        private readonly CSharpMetricExtractor _sut;
        private Extractor<CSharpMetricExtractor> _extractor;

        public CSharpMetricsTests()
        {
            _sut = new UsingsCountMetric();
        }

        [Fact]
        public void
            Extract_ShouldHaveUsingsCountMetricAndCorrectClassNamesWithNamespaces_WhenGivenMultipleUsingsAtMultipleLevels()
        {
            const string fileContent = @"using System;
                                    using System.Collections.Generic;
                                    using System.Linq;
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
            Assert.Equal(10, count);

            Assert.Equal(2, compilationUnitModel.Entities.Count);
            
            var foo = compilationUnitModel.Entities[0];
            Assert.Equal("Foo", foo.Name);
            Assert.Equal("TopLevel.Child1", foo.Namespace);
            
            var bar = compilationUnitModel.Entities[1];
            Assert.Equal("Bar", bar.Name);
            Assert.Equal("TopLevel.Child2", bar.Namespace);
        }
    }
}