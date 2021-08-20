using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpParameterRelationMetricTests
    {
        private readonly ParameterRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpParameterRelationMetricTests()
        {
            _sut = new ParameterRelationVisitor(new RelationMetricHolder());

            var visitorList = new VisitorList();
            visitorList.Add(new ClassSetterCompilationUnitVisitor(new CSharpClassModelCreator(
                new List<ICSharpClassVisitor>
                {
                    new BaseInfoClassVisitor(),
                    _sut
                })));
            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), visitorList);
        }

        [Fact]
        public void PrettyPrint_ShouldReturnParameterDependency()
        {
            Assert.Equal("Parameter Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveNoParameters_WhenClassHasMethodsWithNoParameters()
        {
            const string fileContent = @"
                                     namespace App
                                     {                                       

                                         class MyClass
                                         {                                           
                                             public void Foo() { }

                                             public void Foo() { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dictionary = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;
            Assert.Empty(dictionary);
        }

        [Fact]
        public void Extract_ShouldHaveNoParameters_WhenClassHasConstructorWithNoParameters()
        {
            const string fileContent = @"
                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public MyClass() { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dictionary = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;
            Assert.Empty(dictionary);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveParameters_WhenClassHasMethodsWithPrimitiveParameters()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public void Foo(int a, float b, string c) { }

                                             public void Bar(float a, int b) { }

                                             public void Zoo(int a) { }

                                             public void Goo() { }
                                         }
                                     }";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(2, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveParameters_WhenInterfaceHasMethodsWithPrimitiveParameters()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public void Foo(int a, float b, string c);

                                             public void Bar(float a, int b);

                                             public string Zoo(int a);

                                             public int Goo();
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(2, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenInterfaceHasMethodsWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public void Foo(int a, CSharpMetricExtractor extractor) ;

                                             public void Bar(IFactExtractor factExtractor,  CSharpMetricExtractor extractor) ;
                                         }
                                     }";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(1, dependencies["int"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasMethodsWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public void Foo(int a, CSharpMetricExtractor extractor, string name) { }

                                             public void Bar(IFactExtractor factExtractor,  CSharpMetricExtractor extractor, int b) { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasConstructorWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public Class1(int a, CSharpMetricExtractor extractor, string name) { }

                                             public Class1(IFactExtractor factExtractor,  CSharpMetricExtractor extractor, int b) { }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["string"]);
        }
    }
}
