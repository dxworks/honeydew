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
    public class CSharpLocalVariablesRelationVisitorTests
    {
        private readonly LocalVariablesRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpLocalVariablesRelationVisitorTests()
        {
            _sut = new LocalVariablesRelationVisitor(new RelationMetricHolder());

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
        public void PrettyPrint_ShouldReturnLocalVariablesDependency()
        {
            Assert.Equal("Local Variables Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveNoLocalVariables_WhenClassHasMethodsThatDontUseLocalVariables()
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
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Empty(dependencies);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveLocalValues_WhenClassHasMethodsThatHaveLocalVariables()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public int Foo(int a, float b, string c) { int x=5;int k=a*x;}

                                             public float Bar(float a, int b) { float k=a*b; return k;}

                                             public int Zoo(int a) {int b = a*124; return b; }

                                             public string Goo() { var f =""Hello""; return f; }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHaveNoPrimitiveLocalVariables_WhenGivenAnInterface()
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
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Empty(dependencies);
        }

        [Fact]
        public void Extract_ShouldHaveNoDependencies_WhenGivenAnInterface()
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
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Empty(dependencies);
        }

        [Fact]
        public void Extract_ShouldHaveLocalVariablesDependencies_WhenClassHasMethodsWithNonPrimitiveLocalVariables()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, string name) { var b = new CSharpMetricExtractor(); return b;}

                                             public IFactExtractor Bar(CSharpMetricExtractor extractor, int b) {IFactExtractor a; IFactExtractor b; return null; }

                                             public IFactExtractor Goo(CSharpMetricExtractor extractor) { IFactExtractor a; CSharpMetricExtractor k; return null;}
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(3, dependencies["IFactExtractor"]);
        }

        [Fact]
        public void Extract_ShouldHaveLocalVariablesDependencies_WhenClassHasConstructorLocalVariables()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                       
                                              int _a; string b;    
                                             public Class1(int a, string name) {_a=a; var c = new CSharpMetricExtractor(); var x = a+2; b=name+x;}

                                             public Class1() { var i=0; var c=2; _a=i+c;  var x = _a+2; b=""name""+x;}

                                             double f() { int a=2; var c=6.0; return a+c; }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(5, dependencies["int"]);
            Assert.Equal(1, dependencies["double"]);
        }

        [Fact]
        public void
            Extract_ShouldHaveLocalVariablesDependencies_WhenClassHasMethodsWithNonPrimitiveLocalVariablesInAForLoop()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, string name) {
                                                 for (var i=0;i<a;i++) { 
                                                  if (name == ""AA"") {
                                                 var b = new CSharpMetricExtractor(); return b;}
                                                 }
                                             return null;
                                             }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["int"]);
        }


        [Fact]
        public void
            Extract_ShouldHaveLocalVariablesDependencies_WhenNamespaceHasMultipleClasses()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, string name) {
                                                 for (var i=0;i<a;i++) { 
                                                  if (name == ""AA"") {
                                                 var b = new CSharpMetricExtractor(); return b;}
                                                 }
                                             return null;
                                             }
                                         }

                                        public class Class2
                                         {                                       
                                              int _a; string b;    
                                             public Class2(int a, string name) {_a=a; var c = new CSharpMetricExtractor(); var x = a+2; b=name+x;}

                                             public Class2() { var i=0; var c=2; _a=i+c;  var x = _a+2; b=""name""+x;}

                                             double f() { int a=2; var c=6.0; return a+c; }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies1 = (IDictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies1.Count);
            Assert.Equal(1, dependencies1["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies1["int"]);


            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.LocalVariablesRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[1].Metrics[0].ValueType);

            var dependencies2 = (IDictionary<string, int>)classTypes[1].Metrics[0].Value;

            Assert.Equal(3, dependencies2.Count);
            Assert.Equal(1, dependencies2["CSharpMetricExtractor"]);
            Assert.Equal(5, dependencies2["int"]);
            Assert.Equal(1, dependencies2["double"]);
        }
    }
}
