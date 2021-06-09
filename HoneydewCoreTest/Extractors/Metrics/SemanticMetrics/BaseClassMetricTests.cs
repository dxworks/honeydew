using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SemanticMetrics
{
    public class BaseClassMetricTests
    {
        private readonly CSharpMetricExtractor _sut;
        private Extractor<CSharpMetricExtractor> _extractor;

        public BaseClassMetricTests()
        {
            _sut = new BaseClassMetric();
        }
        
        [Fact]
        public void GetMetricType_ShouldReturnSemantic()
        {
            Assert.Equal(MetricType.Semantic, _sut.GetMetricType());
        }


        [Fact]
        public void Extract_ShouldHaveBaseClassObject_WhenClassDoesNotExtendsAnyClass()
        {
            const string fileContent = @"using System;

                                    namespace App
                                    {                                       

                                        class MyClass
                                        {                                           
                                            public void Foo() { }
                                        }
                                    }";


            var metrics = new List<CSharpMetricExtractor>()
            {
                _sut
            };

            _extractor = new CSharpClassExtractor(metrics);

            var compilationUnitModel = _extractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(1, compilationUnitModel.Entities.Count);

            var classModel = compilationUnitModel.Entities[0];

            Assert.Equal("MyClass", classModel.Name);

            var metric = classModel.Metrics.Get<InheritanceMetric>();

            Assert.True(metric.HasValue);

            var baseClassMetric = metric.Value;
            var inheritanceMetric = baseClassMetric.Value;

            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);
        }

        [Fact]
        public void Extract_ShouldHaveBaseClassIMetric_WhenClassExtendsIMetricInterface()
        {
            const string fileContent = @"using System;
                                    using HoneydewCore.Extractors.Metrics;
                                    namespace App
                                    {                                       

                                        namespace Domain{
                                        class MyClass : IMetric
                                        {                                           
                                            public void Foo() { }
                                            public void Bar() { }
                                        }}
                                    }";


            var metrics = new List<CSharpMetricExtractor>()
            {
                _sut
            };

            _extractor = new CSharpClassExtractor(metrics);

            var compilationUnitModel = _extractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(1, compilationUnitModel.Entities.Count);

            var classModel = compilationUnitModel.Entities[0];

            Assert.Equal("MyClass", classModel.Name);

            var metric = classModel.Metrics.Get<InheritanceMetric>();

            Assert.True(metric.HasValue);

            var baseClassMetric = metric.Value;
            var inheritanceMetric = baseClassMetric.Value;

            Assert.Equal(1, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);
            Assert.Equal("IMetric", inheritanceMetric.Interfaces[0]);
        }

        [Fact]
        public void Extract_ShouldHaveBaseObjectAndNoInterfaces_WhenClassOnlyExtendsOtherClass()
        {
            const string fileContent = @"using System;
                                    using HoneydewCore.Extractors.Metrics;
                                    namespace App
                                    {                                       
                                        class Parent {}    

                                        class ChildClass : Parent
                                        {                                           
                                           
                                        }
                                    }";


            var metrics = new List<CSharpMetricExtractor>()
            {
                _sut
            };

            _extractor = new CSharpClassExtractor(metrics);

            var compilationUnitModel = _extractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(2, compilationUnitModel.Entities.Count);

            var parentClassModel = compilationUnitModel.Entities[0];

            var inheritanceMetric = parentClassModel.Metrics.Get<InheritanceMetric>().Value.Value;

            Assert.Equal("Parent", parentClassModel.Name);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);

            var classModel = compilationUnitModel.Entities[1];

            Assert.Equal("ChildClass", classModel.Name);

            var metric = classModel.Metrics.Get<InheritanceMetric>();
            Assert.True(metric.HasValue);

            var baseClassMetric = metric.Value.Value;

            Assert.Equal("Parent", baseClassMetric.BaseClassName);
            Assert.Equal(0, baseClassMetric.Interfaces.Count);
        }

        [Fact]
        public void
            Extract_ShouldHaveBaseObjectAndInterfaces_WhenClassExtendsOtherClassAndImplementsMultipleInterfaces()
        {
            const string fileContent = @"using System;
                                    using HoneydewCore.Extractors.Metrics;
                                    namespace App
                                    {                                       
                                        class Parent {}    

                                        class ChildClass : Parent, IMetric, IMetricExtractor
                                        {                                           
                                            public MetricType GetMetricType() {return null;}

                                            public string GetName(){return null;}

                                            public IMetric GetMetric(){return null;}
                                        }
                                    }";


            var metrics = new List<CSharpMetricExtractor>()
            {
                _sut
            };

            _extractor = new CSharpClassExtractor(metrics);

            var compilationUnitModel = _extractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(2, compilationUnitModel.Entities.Count);

            var parentClassModel = compilationUnitModel.Entities[0];

            var inheritanceMetric = parentClassModel.Metrics.Get<InheritanceMetric>().Value.Value;

            Assert.Equal("Parent", parentClassModel.Name);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);

            var classModel = compilationUnitModel.Entities[1];

            Assert.Equal("ChildClass", classModel.Name);

            var metric = classModel.Metrics.Get<InheritanceMetric>();
            Assert.True(metric.HasValue);

            var baseClassMetric = metric.Value.Value;

            Assert.Equal("Parent", baseClassMetric.BaseClassName);
            Assert.Equal(2, baseClassMetric.Interfaces.Count);
            
            Assert.Equal("IMetric", baseClassMetric.Interfaces[0]);
            Assert.Equal("IMetricExtractor", baseClassMetric.Interfaces[1]);
        }
    }
}