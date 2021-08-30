using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.CompilationUnitLevel
{
    public class CSharpImportsCountMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpImportsCountMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();
            
            compositeVisitor.Add(new ImportCountCompilationUnitVisitor());

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
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

            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(1, compilationUnit.Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit.ImportCountCompilationUnitVisitor",
                compilationUnit.Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", compilationUnit.Metrics[0].ValueType);
            Assert.Equal(6, compilationUnit.Metrics[0].Value);
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

            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(1, compilationUnit.Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit.ImportCountCompilationUnitVisitor",
                compilationUnit.Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", compilationUnit.Metrics[0].ValueType);
            Assert.Equal(12, compilationUnit.Metrics[0].Value);
        }
    }
}
