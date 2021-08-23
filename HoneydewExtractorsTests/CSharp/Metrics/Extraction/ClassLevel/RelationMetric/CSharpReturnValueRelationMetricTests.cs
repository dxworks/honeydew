using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpReturnValueRelationMetricTests
    {
        private readonly ReturnValueRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpReturnValueRelationMetricTests()
        {
            _sut = new ReturnValueRelationVisitor(new RelationMetricHolder());

            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                _sut
            }));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Fact]
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Return Value Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveVoidReturnValues_WhenClassHasMethodsThatReturnVoid()
        {
            const string fileContent = @"
                                     namespace App
                                     {                                       

                                         class MyClass
                                         {                                           
                                             public void Foo() { }

                                             public void Bar() { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Single(dependencies);
            Assert.Equal(2, dependencies["void"]);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveReturnValues_WhenClassHasMethodsThatReturnPrimitiveValues()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public int Foo(int a, float b, string c) { }

                                             public float Bar(float a, int b) { }

                                             public int Zoo(int a) { }

                                             public string Goo() { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveReturnValues_WhenInterfaceHasMethodsWithPrimitiveReturnValues()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public float Foo(int a, float b, string c);

                                             public void Bar(float a, int b);

                                             public string Zoo(int a);

                                             public int Goo();
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;
            Assert.Equal(4, dependencies.Count);
            Assert.Equal(1, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
            Assert.Equal(1, dependencies["void"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesReturnValues_WhenInterfaceHasMethodsWithDependenciesReturnValues()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, CSharpMetricExtractor extractor) ;

                                             public CSharpMetricExtractor Foo(int a) ;

                                             public IFactExtractor Bar(CSharpMetricExtractor extractor) ;
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesReturnValues_WhenClassHasMethodsWithDependenciesReturnValues()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class IInterface
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, string name) { }

                                             public IFactExtractor Bar(CSharpMetricExtractor extractor, int b) { }

                                             public IFactExtractor Goo(CSharpMetricExtractor extractor) { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(2, dependencies["IFactExtractor"]);
        }
    }
}
