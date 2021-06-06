using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics
{
    public class CSharpMetricsTests
    {
        private Extractor<CSharpMetricExtractor> _sut;


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

                                            class Foo { }
                                        }

                                        namespace Child2
                                        {
                                            using System.CodeDom;
                                            using Microsoft.CSharp;

                                            class Bar { }
                                        }
                                    }";

            var usingsCountMetric = new UsingsCountMetric();
            var metrics = new List<CSharpMetricExtractor>()
            {
                usingsCountMetric
            };

            _sut = new CSharpClassExtractor(metrics);

            var entity = _sut.Extract(fileContent);
            Assert.Equal(typeof(ClassModel), entity.GetType());

            var projectClass = (ClassModel) entity;

            Assert.NotEmpty(projectClass.Metrics);
            Assert.True(projectClass.Metrics.TryGetValue(usingsCountMetric.GetName(), out var count));
            Assert.Equal(12, count);
        }
    }
}