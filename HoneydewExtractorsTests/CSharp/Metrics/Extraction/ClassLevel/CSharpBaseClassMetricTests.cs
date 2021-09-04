using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpBaseClassMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpBaseClassMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor()
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classType = classTypes[0];

            Assert.Equal("App.MyClass", classType.Name);
            Assert.Equal(1, classType.BaseTypes.Count);
            Assert.Equal("object", classType.BaseTypes[0].Type.Name);
            Assert.Equal("class", classType.BaseTypes[0].Kind);
        }

        [Fact]
        public void Extract_ShouldHaveBaseClassIMetric_WhenClassExtendsIMetricInterface()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       

                                         namespace Domain{
                                         class MyClass : IMetric, IMetric2
                                         {                                           
                                             public void Foo() { }
                                             public void Bar() { }
                                         }}
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classType = classTypes[0];

            Assert.Equal("App.Domain.MyClass", classType.Name);

            var baseTypes = classType.BaseTypes;

            Assert.Equal(2, baseTypes.Count);
            Assert.Equal("IMetric", baseTypes[0].Type.Name);
            Assert.Equal("class", baseTypes[0].Kind);
            
            Assert.Equal("IMetric2", baseTypes[1].Type.Name);
            Assert.Equal("interface", baseTypes[1].Kind);
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


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var baseClassType = classTypes[0];
            var baseClassBaseTypes = baseClassType.BaseTypes;

            Assert.Equal("App.Parent", baseClassType.Name);
            Assert.Equal(1, baseClassBaseTypes.Count);
            Assert.Equal("object", baseClassBaseTypes[0].Type.Name);
            Assert.Equal("class", baseClassBaseTypes[0].Kind);

            var classType = classTypes[1];
            var baseTypes = classType.BaseTypes;

            Assert.Equal("App.ChildClass", classType.Name);
            Assert.Equal(1, baseTypes.Count);
            Assert.Equal("App.Parent", baseTypes[0].Type.Name);
            Assert.Equal("class", baseTypes[0].Kind);
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var baseClassType = classTypes[0];
            var baseClassBaseTypes = baseClassType.BaseTypes;

            Assert.Equal("App.Parent", baseClassType.Name);
            Assert.Equal(1, baseClassBaseTypes.Count);
            Assert.Equal("object", baseClassBaseTypes[0].Type.Name);
            Assert.Equal("class", baseClassBaseTypes[0].Kind);

            var classType = classTypes[1];
            var baseTypes = classType.BaseTypes;

            Assert.Equal("App.ChildClass", classType.Name);
            Assert.Equal(3, baseTypes.Count);
            Assert.Equal("App.Parent", baseTypes[0].Type.Name);
            Assert.Equal("class", baseTypes[0].Kind);

            Assert.Equal("IMetric", baseTypes[1].Type.Name);
            Assert.Equal("interface", baseTypes[1].Kind);

            Assert.Equal("IMetricExtractor", baseTypes[2].Type.Name);
            Assert.Equal("interface", baseTypes[2].Kind);
        }
    }
}
