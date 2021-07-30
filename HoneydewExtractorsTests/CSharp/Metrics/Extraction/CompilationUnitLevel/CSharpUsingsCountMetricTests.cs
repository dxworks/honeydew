using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.CompilationUnitLevel
{
    public class CSharpUsingsCountMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpUsingsCountMetricTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpUsingsCountMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnCompilationUnitLevel()
        {
            Assert.Equal(ExtractionMetricType.CompilationUnitLevel, new CSharpUsingsCountMetric().GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnUsingsCount()
        {
            Assert.Equal("Usings Count", new CSharpUsingsCountMetric().PrettyPrint());
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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpUsingsCountMetric>();
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

            var classModels = _factExtractor.Extract(fileContent);
            Assert.Equal(2, classModels.Count);

            var optional1 = classModels[0].GetMetricValue<CSharpUsingsCountMetric>();
            Assert.True(optional1.HasValue);
            Assert.Equal(12, (int) optional1.Value);

            var optional2 = classModels[0].GetMetricValue<CSharpUsingsCountMetric>();
            Assert.True(optional2.HasValue);
            Assert.Equal(12, (int) optional2.Value);
        }
    }
}
