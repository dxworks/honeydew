using System;
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
        private IFactExtractor _factExtractor;

        public BaseClassMetricTests()
        {
            _sut = new BaseClassMetric();
        }
        
        [Fact]
        public void GetMetricType_ShouldReturnSemantic()
        {
            Assert.True(_sut is ISemanticMetric);
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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            Assert.Equal(1, classModels[0].Metrics.Count);

            var classModel = classModels[0];

            Assert.Equal("App.MyClass", classModel.FullName);

            var optional = classModel.GetMetric<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var inheritanceMetric = (InheritanceMetric) optional.Value;

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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var classModel = classModels[0];

            Assert.Equal(1, classModel.Metrics.Count);
            
            Assert.Equal("App.Domain.MyClass", classModel.FullName);

            var optional = classModel.GetMetric<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var inheritanceMetric = (InheritanceMetric)optional.Value;

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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);


            Assert.Equal(2, classModels.Count);

            var parentClassModel = classModels[0];
            Assert.Equal(1, parentClassModel.Metrics.Count);
            
            var parentOptional = parentClassModel.GetMetric<BaseClassMetric>();
            
            Assert.True(parentOptional.HasValue);

            var inheritanceMetric = (InheritanceMetric) parentOptional.Value;

            Assert.Equal("App.Parent", parentClassModel.FullName);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);

            var classModel = classModels[1];
            
            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("App.ChildClass", classModel.FullName);

            var optional = classModel.GetMetric<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var baseClassMetric = (InheritanceMetric) optional.Value;

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


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };
            
            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            var parentClassModel = classModels[0];
            
            Assert.Equal(1, parentClassModel.Metrics.Count);
            
            var parentOptional = parentClassModel.GetMetric<BaseClassMetric>();
            Assert.True(parentOptional.HasValue);
            
            var inheritanceMetric = (InheritanceMetric) parentOptional.Value;
            
            Assert.Equal("App.Parent", parentClassModel.FullName);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric.BaseClassName);

            var classModel = classModels[1];
            Assert.Equal(1, classModel.Metrics.Count);

            Assert.Equal("App.ChildClass", classModel.FullName);

            var optional = classModel.GetMetric<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var baseClassMetric = (InheritanceMetric) optional.Value;

            Assert.Equal("Parent", baseClassMetric.BaseClassName);
            Assert.Equal(2, baseClassMetric.Interfaces.Count);
            
            Assert.Equal("IMetric", baseClassMetric.Interfaces[0]);
            Assert.Equal("IMetricExtractor", baseClassMetric.Interfaces[1]);
        }
    }
}