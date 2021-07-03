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
        public void PrettyPrint_ShouldReturnInheritsClass()
        {
            Assert.Equal("Inherits Class", _sut.PrettyPrint());
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

            var optional = classModel.GetMetricValue<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var inheritanceMetric = (InheritanceMetric) optional.Value;

            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("object", inheritanceMetric.BaseClassName);
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

            var optional = classModel.GetMetricValue<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var inheritanceMetric = (InheritanceMetric)optional.Value;

            Assert.Equal(1, inheritanceMetric.Interfaces.Count);
            Assert.Equal("object", inheritanceMetric.BaseClassName);
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

            var baseclassModel = classModels[0];
            Assert.Equal(1, baseclassModel.Metrics.Count);
            
            var parentOptional = baseclassModel.GetMetricValue<BaseClassMetric>();
            
            Assert.True(parentOptional.HasValue);

            var inheritanceMetric = (InheritanceMetric) parentOptional.Value;

            Assert.Equal("App.Parent", baseclassModel.FullName);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("object", inheritanceMetric.BaseClassName);

            var classModel = classModels[1];
            
            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("App.ChildClass", classModel.FullName);

            var optional = classModel.GetMetricValue<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var baseClassMetric = (InheritanceMetric) optional.Value;

            Assert.Equal("App.Parent", baseClassMetric.BaseClassName);
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

            var baseClassModel = classModels[0];
            
            Assert.Equal(1, baseClassModel.Metrics.Count);
            
            var parentOptional = baseClassModel.GetMetricValue<BaseClassMetric>();
            Assert.True(parentOptional.HasValue);
            
            var inheritanceMetric = (InheritanceMetric) parentOptional.Value;
            
            Assert.Equal("App.Parent", baseClassModel.FullName);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("object", inheritanceMetric.BaseClassName);

            var classModel = classModels[1];
            Assert.Equal(1, classModel.Metrics.Count);

            Assert.Equal("App.ChildClass", classModel.FullName);

            var optional = classModel.GetMetricValue<BaseClassMetric>();
            Assert.True(optional.HasValue);

            var baseClassMetric = (InheritanceMetric) optional.Value;

            Assert.Equal("App.Parent", baseClassMetric.BaseClassName);
            Assert.Equal(2, baseClassMetric.Interfaces.Count);
            
            Assert.Equal("IMetric", baseClassMetric.Interfaces[0]);
            Assert.Equal("IMetricExtractor", baseClassMetric.Interfaces[1]);
        }
    }
}