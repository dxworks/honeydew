using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SemanticMetrics
{
    public class LocalVariablesDependencyMetricTests
    {
        private readonly LocalVariablesDependencyMetric _sut;
        private readonly IFactExtractor _factExtractor;

        public LocalVariablesDependencyMetricTests()
        {
            _sut = new LocalVariablesDependencyMetric();
            _factExtractor = new CSharpClassFactExtractor(new List<Type>
            {
                _sut.GetType()
            });
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dataMetric = (DependencyDataMetric) optional.Value;

            Assert.Empty(dataMetric.Usings);
            Assert.Empty(dataMetric.Dependencies);
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dataMetric = (DependencyDataMetric) optional.Value;

            Assert.Equal(1, dataMetric.Usings.Count);
            Assert.Equal("System", dataMetric.Usings[0]);

            Assert.Equal(3, dataMetric.Dependencies.Count);
            Assert.Equal(3, dataMetric.Dependencies["int"]);
            Assert.Equal(1, dataMetric.Dependencies["float"]);
            Assert.Equal(1, dataMetric.Dependencies["string"]);
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dataMetric = (DependencyDataMetric) optional.Value;

            Assert.Equal(1, dataMetric.Usings.Count);
            Assert.Equal("System", dataMetric.Usings[0]);

            Assert.Empty(dataMetric.Dependencies);
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dataMetric = (DependencyDataMetric) optional.Value;

            Assert.Equal(4, dataMetric.Usings.Count);
            Assert.Equal("System", dataMetric.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dataMetric.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dataMetric.Usings[2]);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics", dataMetric.Usings[3]);

            Assert.Empty(dataMetric.Dependencies);
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dataMetric = (DependencyDataMetric) optional.Value;

            Assert.Equal(3, dataMetric.Usings.Count);
            Assert.Equal("System", dataMetric.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dataMetric.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dataMetric.Usings[2]);

            Assert.Equal(2, dataMetric.Dependencies.Count);
            Assert.Equal(2, dataMetric.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(3, dataMetric.Dependencies["IFactExtractor"]);
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dataMetric = (DependencyDataMetric) optional.Value;

            Assert.Equal(3, dataMetric.Usings.Count);
            Assert.Equal("System", dataMetric.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dataMetric.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dataMetric.Usings[2]);

            Assert.Equal(3, dataMetric.Dependencies.Count);
            Assert.Equal(1, dataMetric.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(5, dataMetric.Dependencies["int"]);
            Assert.Equal(1, dataMetric.Dependencies["double"]);
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

            var optional = classModels[0].GetMetricValue<LocalVariablesDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(2, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dependencies.Usings[1]);

            Assert.Equal(2, dependencies.Dependencies.Count);
            Assert.Equal(1, dependencies.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies.Dependencies["int"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasMethodsWithNoReturnValues()
        {
            var fileRelations = _sut.GetRelations(new DependencyDataMetric());

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenDependenciesAreOnlyPrimitiveTypes()
        {
            var fileRelations = _sut.GetRelations(new DependencyDataMetric
            {
                Usings = {"System"},
                Dependencies =
                {
                    {"int", 3},
                    {"float", 2},
                    {"string", 1}
                }
            });

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_Extract_ShouldHaveRelations_WhenThereAreNonPrimitiveDependencies()
        {
            var fileRelations = _sut.GetRelations(new DependencyDataMetric
            {
                Usings =
                {
                    "System", "HoneydewCore.Extractors", "HoneydewCore.Extractors.Metrics",
                    "HoneydewCore.Extractors.Metrics.SemanticMetrics"
                },
                Dependencies =
                {
                    {"int", 3},
                    {"IFactExtractor", 2},
                    {"CSharpMetricExtractor", 1}
                }
            });

            Assert.NotEmpty(fileRelations);
            Assert.Equal(2, fileRelations.Count);

            var fileRelation1 = fileRelations[0];
            Assert.Equal("IFactExtractor", fileRelation1.FileTarget);
            Assert.Equal(typeof(LocalVariablesDependencyMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(LocalVariablesDependencyMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}