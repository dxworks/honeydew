﻿using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SemanticMetrics
{
    public class BaseClassMetricTests
    {
        private readonly CSharpMetricExtractor _sut;
        private IFactExtractor _factExtractor;

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

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(1, compilationUnitModel.ClassModels.Count);

            var classModel = compilationUnitModel.ClassModels[0];

            Assert.Equal("MyClass", classModel.Name);

            var optional = classModel.Metrics.Get<BaseClassMetric>();
            Assert.True(optional.HasValue);
            var metric = (Metric<InheritanceMetric>) optional.Value;
            
            var inheritanceMetric = metric.Value;

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

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(1, compilationUnitModel.ClassModels.Count);

            var classModel = compilationUnitModel.ClassModels[0];

            Assert.Equal("MyClass", classModel.Name);

            var optional = classModel.Metrics.Get<BaseClassMetric>();
            Assert.True(optional.HasValue);
            var metric = (Metric<InheritanceMetric>) optional.Value;

            var inheritanceMetric = metric.Value;

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

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(2, compilationUnitModel.ClassModels.Count);

            var parentClassModel = compilationUnitModel.ClassModels[0];
            
            var parentOptional = parentClassModel.Metrics.Get<BaseClassMetric>();
            Assert.True(parentOptional.HasValue);

            var inheritanceMetric = ((Metric<InheritanceMetric>) parentOptional.Value).Value;

            Assert.Equal("Parent", parentClassModel.Name);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);

            var classModel = compilationUnitModel.ClassModels[1];

            Assert.Equal("ChildClass", classModel.Name);

            var optional = classModel.Metrics.Get<BaseClassMetric>();
            Assert.True(optional.HasValue);
            var metric = (Metric<InheritanceMetric>) optional.Value;

            var baseClassMetric = metric.Value;

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

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var compilationUnitModel = _factExtractor.Extract(fileContent);

            Assert.False(compilationUnitModel.SyntacticMetrics.HasMetrics());

            Assert.Equal(2, compilationUnitModel.ClassModels.Count);

            var parentClassModel = compilationUnitModel.ClassModels[0];

            
            var parentOptional = parentClassModel.Metrics.Get<BaseClassMetric>();
            Assert.True(parentOptional.HasValue);

            var inheritanceMetric = ((Metric<InheritanceMetric>) parentOptional.Value).Value;

            Assert.Equal("Parent", parentClassModel.Name);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);

            var classModel = compilationUnitModel.ClassModels[1];

            Assert.Equal("ChildClass", classModel.Name);

            var optional = classModel.Metrics.Get<BaseClassMetric>();
            Assert.True(optional.HasValue);
            var metric = (Metric<InheritanceMetric>) optional.Value;

            var baseClassMetric = metric.Value;

            Assert.Equal("Parent", baseClassMetric.BaseClassName);
            Assert.Equal(2, baseClassMetric.Interfaces.Count);
            
            Assert.Equal("IMetric", baseClassMetric.Interfaces[0]);
            Assert.Equal("IMetricExtractor", baseClassMetric.Interfaces[1]);
        }
    }
}