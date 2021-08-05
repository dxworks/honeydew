using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpLocalVariablesRelationMetricTests
    {
        private readonly CSharpLocalVariablesRelationMetric _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpLocalVariablesRelationMetricTests()
        {
            _sut = new CSharpLocalVariablesRelationMetric();
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpLocalVariablesRelationMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, _sut.GetMetricType());
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            var optional1 = classModels[0].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional1.HasValue);

            var dependencies1 = (IDictionary<string, int>) optional1.Value;

            Assert.Equal(2, dependencies1.Count);
            Assert.Equal(1, dependencies1["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies1["int"]);


            var optional2 = classModels[1].GetMetricValue<CSharpLocalVariablesRelationMetric>();
            Assert.True(optional2.HasValue);

            var dependencies2 = (IDictionary<string, int>) optional2.Value;

            Assert.Equal(3, dependencies2.Count);
            Assert.Equal(1, dependencies2["CSharpMetricExtractor"]);
            Assert.Equal(5, dependencies2["int"]);
            Assert.Equal(1, dependencies2["double"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasMethodsWithNoReturnValues()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>());

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenDependenciesAreOnlyPrimitiveTypes()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>
            {
                {"int", 3},
                {"float", 2},
                {"string", 1}
            });

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_Extract_ShouldHaveRelations_WhenThereAreNonPrimitiveDependencies()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>
            {
                {"int", 3},
                {"IFactExtractor", 2},
                {"CSharpMetricExtractor", 1}
            });

            Assert.NotEmpty(fileRelations);
            Assert.Equal(2, fileRelations.Count);

            var fileRelation1 = fileRelations[0];
            Assert.Equal("IFactExtractor", fileRelation1.FileTarget);
            Assert.Equal(typeof(CSharpLocalVariablesRelationMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(CSharpLocalVariablesRelationMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}
