using System;
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
            Assert.True(_sut is ISyntacticMetric);
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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            
            var optional = classModels[0].GetMetric<UsingsCountMetric>();
            Assert.True(optional.HasValue);
            var count = (int) optional.Value;

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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);
            Assert.Equal(2, classModels.Count);
            
            var optional1 = classModels[0].GetMetric<UsingsCountMetric>();
            Assert.True(optional1.HasValue);
            Assert.Equal(12, (int) optional1.Value);
            
            var optional2 = classModels[0].GetMetric<UsingsCountMetric>();
            Assert.True(optional2.HasValue);
            Assert.Equal(12, (int) optional2.Value);
        }
    }
}