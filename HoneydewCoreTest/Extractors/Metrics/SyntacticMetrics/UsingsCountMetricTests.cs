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
        private IFactExtractor _factExtractor;

        public UsingsCountMetricTests()
        {
            _sut = new UsingsCountMetric();
        }

        [Fact]
        public void GetMetricType_ShouldReturnSyntactic()
        {
            Assert.Equal(MetricType.Syntactic, _sut.GetMetricType());
        }
        
         
        [Fact]
        public void Extract_ShouldHaveUsingsCountMetric_WhenGivenOneUsingsLevel()
        {
            const string fileContent = @"using System;
                                    using System.Collections.Generic;
                                    using System.Linq;
                                    using System.Text;
                                    using Microsoft.CodeAnalysis;
                                    using Microsoft.CodeAnalysis.CSharp;

                                    namespace TopLevel
                                    {
                                        public class Foo { int a; public void f(); }                                        
                                    }";

            
            var metrics = new List<CSharpMetricExtractor>()
            {
                _sut
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.True(compilationUnitModel.SyntacticMetrics.HasMetrics());

            var optional = compilationUnitModel.SyntacticMetrics.Get<int>();
            Assert.True(optional.HasValue);
            
            var metric = optional.Value;
            var count = metric.Value;

            Assert.Equal(6, count);
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

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.True(compilationUnitModel.SyntacticMetrics.HasMetrics());

            var optional = compilationUnitModel.SyntacticMetrics.Get<int>();

            Assert.True(optional.HasValue);
            
            var metric = optional.Value;

            Assert.Equal(12, metric.Value);
        }
    }
}