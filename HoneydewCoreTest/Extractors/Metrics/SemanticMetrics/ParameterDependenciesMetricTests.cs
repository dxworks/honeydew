﻿using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SemanticMetrics
{
    public class ParameterDependenciesMetricTests
    {
        private readonly ParameterDependenciesMetric _sut;
        private IFactExtractor _factExtractor;

        public ParameterDependenciesMetricTests()
        {
            _sut = new ParameterDependenciesMetric();
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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetric<ParameterDependenciesMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Empty(dependencies.Dependencies);
            Assert.Empty(dependencies.Usings);
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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetric<ParameterDependenciesMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(1, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);

            Assert.Equal(3, dependencies.Dependencies.Count);
            Assert.Equal(3, dependencies.Dependencies["int"]);
            Assert.Equal(2, dependencies.Dependencies["float"]);
            Assert.Equal(1, dependencies.Dependencies["string"]);
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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetric<ParameterDependenciesMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(1, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);

            Assert.Equal(3, dependencies.Dependencies.Count);
            Assert.Equal(3, dependencies.Dependencies["int"]);
            Assert.Equal(2, dependencies.Dependencies["float"]);
            Assert.Equal(1, dependencies.Dependencies["string"]);
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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetric<ParameterDependenciesMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(4, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dependencies.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dependencies.Usings[2]);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics", dependencies.Usings[3]);

            Assert.Equal(3, dependencies.Dependencies.Count);
            Assert.Equal(2, dependencies.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies.Dependencies["IFactExtractor"]);
            Assert.Equal(1, dependencies.Dependencies["int"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasMethodsWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                    using HoneydewCore.Extractors;
                                    using HoneydewCore.Extractors.Metrics;
                                    namespace App
                                    {                                       
                                        public class IInterface
                                        {                                           
                                            public void Foo(int a, CSharpMetricExtractor extractor, string name) { }

                                            public void Bar(IFactExtractor factExtractor,  CSharpMetricExtractor extractor, int b) { }
                                        }
                                    }";


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetric<ParameterDependenciesMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (DependencyDataMetric) optional.Value;

            Assert.Equal(3, dependencies.Usings.Count);
            Assert.Equal("System", dependencies.Usings[0]);
            Assert.Equal("HoneydewCore.Extractors", dependencies.Usings[1]);
            Assert.Equal("HoneydewCore.Extractors.Metrics", dependencies.Usings[2]);

            Assert.Equal(4, dependencies.Dependencies.Count);
            Assert.Equal(2, dependencies.Dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies.Dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies.Dependencies["int"]);
            Assert.Equal(1, dependencies.Dependencies["string"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasMethodsWithNoParameters()
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
            Assert.Equal("",fileRelation1.FileSource);
            Assert.Equal("IFactExtractor",fileRelation1.FileTarget);
            Assert.Equal(typeof(ParameterDependenciesMetric).FullName,fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("",fileRelation2.FileSource);
            Assert.Equal("CSharpMetricExtractor",fileRelation2.FileTarget);
            Assert.Equal(typeof(ParameterDependenciesMetric).FullName,fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}