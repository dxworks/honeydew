using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpBaseClassMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpBaseClassMetricTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpBaseClassMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, new CSharpBaseClassMetric().GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnInheritsClass()
        {
            Assert.Equal("Inherits Class", new CSharpBaseClassMetric().PrettyPrint());
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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var classModel = classModels[0];

            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("App.MyClass", classModel.FullName);

            var optional = classModel.GetMetricValue<CSharpBaseClassMetric>();
            Assert.True(optional.HasValue);

            var inheritanceMetric = (CSharpInheritanceMetric) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var classModel = classModels[0];

            Assert.Equal(1, classModel.Metrics.Count);

            Assert.Equal("App.Domain.MyClass", classModel.FullName);

            var optional = classModel.GetMetricValue<CSharpBaseClassMetric>();
            Assert.True(optional.HasValue);

            var inheritanceMetric = (CSharpInheritanceMetric) optional.Value;

            Assert.Empty(inheritanceMetric.Interfaces);
            // IMetric instead of object because the Semantic model doesn't know that IMetric is an interface
            Assert.Equal("IMetric", inheritanceMetric.BaseClassName);
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


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            var baseclassModel = classModels[0];
            Assert.Equal(1, baseclassModel.Metrics.Count);

            var parentOptional = baseclassModel.GetMetricValue<CSharpBaseClassMetric>();

            Assert.True(parentOptional.HasValue);

            var inheritanceMetric = (CSharpInheritanceMetric) parentOptional.Value;

            Assert.Equal("App.Parent", baseclassModel.FullName);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("object", inheritanceMetric.BaseClassName);

            var classModel = classModels[1];

            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("App.ChildClass", classModel.FullName);

            var optional = classModel.GetMetricValue<CSharpBaseClassMetric>();
            Assert.True(optional.HasValue);

            var baseClassMetric = (CSharpInheritanceMetric) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            var baseClassModel = classModels[0];

            Assert.Equal(1, baseClassModel.Metrics.Count);

            var parentOptional = baseClassModel.GetMetricValue<CSharpBaseClassMetric>();
            Assert.True(parentOptional.HasValue);

            var inheritanceMetric = (CSharpInheritanceMetric) parentOptional.Value;

            Assert.Equal("App.Parent", baseClassModel.FullName);
            Assert.Equal(0, inheritanceMetric.Interfaces.Count);
            Assert.Equal("object", inheritanceMetric.BaseClassName);

            var classModel = classModels[1];
            Assert.Equal(1, classModel.Metrics.Count);

            Assert.Equal("App.ChildClass", classModel.FullName);

            var optional = classModel.GetMetricValue<CSharpBaseClassMetric>();
            Assert.True(optional.HasValue);

            var baseClassMetric = (CSharpInheritanceMetric) optional.Value;

            Assert.Equal("App.Parent", baseClassMetric.BaseClassName);
            Assert.Equal(2, baseClassMetric.Interfaces.Count);

            Assert.Equal("IMetric", baseClassMetric.Interfaces[0]);
            Assert.Equal("IMetricExtractor", baseClassMetric.Interfaces[1]);
        }
    }
}
