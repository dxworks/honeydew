using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics
{
    public class CSharpMetricsTests // tests with multiple metrics
    {
        private IFactExtractor _factExtractor;

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
            var sut = new UsingsCountMetric();

            var metrics = new List<CSharpMetricExtractor>()
            {
                sut
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.True(compilationUnitModel.SyntacticMetrics.HasMetrics());

            
            var optional = compilationUnitModel.SyntacticMetrics.Get<UsingsCountMetric>();
            Assert.True(optional.HasValue);
            
            var count = (int) optional.Value.GetValue();
            
            Assert.Equal(10, count);

            Assert.Equal(2, compilationUnitModel.ClassModels.Count);

            var foo = compilationUnitModel.ClassModels[0];
            Assert.Equal("Foo", foo.Name);
            Assert.Equal("TopLevel.Child1", foo.Namespace);

            var bar = compilationUnitModel.ClassModels[1];
            Assert.Equal("Bar", bar.Name);
            Assert.Equal("TopLevel.Child2", bar.Namespace);
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

            var metrics = new List<CSharpMetricExtractor>()
            {
                new BaseClassMetric(),
                new UsingsCountMetric()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.True(compilationUnitModel.SyntacticMetrics.HasMetrics());
            var syntacticMetricOptional = compilationUnitModel.SyntacticMetrics.Get<BaseClassMetric>();
            Assert.False(syntacticMetricOptional.HasValue);

            Assert.Equal(2, compilationUnitModel.ClassModels.Count);
            var optional = compilationUnitModel.ClassModels[0].Metrics.Get<UsingsCountMetric>();
            Assert.False(optional.HasValue);
            
            var optional1 = compilationUnitModel.ClassModels[1].Metrics.Get<UsingsCountMetric>();
            Assert.False(optional1.HasValue);
        }
    }
}