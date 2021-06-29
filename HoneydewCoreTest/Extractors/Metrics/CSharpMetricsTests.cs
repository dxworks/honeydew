using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.CompilationUnitMetrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics
{
    public class CSharpMetricsTests // tests with multiple metrics
    {
        private CSharpClassFactExtractor _factExtractor;

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

            _factExtractor = new CSharpClassFactExtractor();
            _factExtractor.AddMetric<UsingsCountMetric>();

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            var foo = classModels[0];
            Assert.Equal("TopLevel.Child1.Foo", foo.FullName);
            Assert.Equal("TopLevel.Child1", foo.Namespace);
            Assert.Equal(1, foo.Metrics.Count);
            var optional1 = foo.GetMetricValue<UsingsCountMetric>();
            Assert.True(optional1.HasValue);
            Assert.Equal(10, (int) optional1.Value);
            

            var bar = classModels[1];
            Assert.Equal("TopLevel.Child2.Bar", bar.FullName);
            Assert.Equal("TopLevel.Child2", bar.Namespace);
            Assert.Equal(1, bar.Metrics.Count);
            var optional2 = bar.GetMetricValue<UsingsCountMetric>();
            Assert.True(optional2.HasValue);
            Assert.Equal(10, (int) optional2.Value);
        }

        [Fact]
        public void Extract_ShouldNotHaveMetric_WhenGivenWrongMetricName()
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

            _factExtractor = new CSharpClassFactExtractor();
            _factExtractor.AddMetric<BaseClassMetric>();
            _factExtractor.AddMetric<UsingsCountMetric>();

            var classModels = _factExtractor.Extract(fileContent);

            // Assert.True(compilationUnitModel.SyntacticMetrics.HasMetrics());
            // var syntacticMetricOptional = compilationUnitModel.SyntacticGetMetric<BaseClassMetric>();
            // Assert.False(syntacticMetricOptional.HasValue);

            Assert.Equal(2, classModels.Count);
            var optional = classModels[0].GetMetricValue<IMetricExtractor>();
            Assert.False(optional.HasValue);
            
            var optional1 = classModels[1].GetMetricValue<IMetricExtractor>();
            Assert.False(optional1.HasValue);
        }
    }
}