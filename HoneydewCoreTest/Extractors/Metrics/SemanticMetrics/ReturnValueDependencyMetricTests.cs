using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SemanticMetrics
{
    public class ReturnValueDependencyMetricTests
    {
        private readonly ReturnValueDependencyMetric _sut;
        private readonly IFactExtractor _factExtractor;

        public ReturnValueDependencyMetricTests()
        {
            _sut = new ReturnValueDependencyMetric();

            _factExtractor = new CSharpClassFactExtractor(new List<Type>
            {
                _sut.GetType()
            });
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<ReturnValueDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Empty(dependencies.Usings);

            Assert.Equal(1, dependencies.Dependencies.Count);
            Assert.Equal(2, dependencies.Dependencies["void"]);
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
            
            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<ReturnValueDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(1, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);

            Assert.Equal(3, dependencies.Dependencies.Count);
            Assert.Equal(2, dependencies.Dependencies["int"]);
            Assert.Equal(1, dependencies.Dependencies["float"]);
            Assert.Equal(1, dependencies.Dependencies["string"]);
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
            
            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<ReturnValueDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(1, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);

            Assert.Equal(4, dependencies.Dependencies.Count);
            Assert.Equal(1, dependencies.Dependencies["int"]);
            Assert.Equal(1, dependencies.Dependencies["float"]);
            Assert.Equal(1, dependencies.Dependencies["string"]);
            Assert.Equal(1, dependencies.Dependencies["void"]);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<ReturnValueDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(4, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dependencies.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dependencies.Usings[2]);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics", dependencies.Usings[3]);

            Assert.Equal(2, dependencies.Dependencies.Count);
            Assert.Equal(2, dependencies.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies.Dependencies["IFactExtractor"]);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<ReturnValueDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(3, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dependencies.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dependencies.Usings[2]);

            Assert.Equal(2, dependencies.Dependencies.Count);
            Assert.Equal(1, dependencies.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(2, dependencies.Dependencies["IFactExtractor"]);
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
            Assert.Equal(typeof(ReturnValueDependencyMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(ReturnValueDependencyMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}