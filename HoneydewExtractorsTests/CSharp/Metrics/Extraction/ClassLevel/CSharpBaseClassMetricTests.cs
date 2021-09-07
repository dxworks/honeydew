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

        [Theory]
        [InlineData("class", "object")]
        [InlineData("struct", "System.ValueType")]
        public void Extract_ShouldHaveBaseClassObject_WhenClassDoesNotExtendsAnyClass(string classType, string baseType)
        {
            var fileContent = $@"using System;

                                    namespace App
                                    {{                                       

                                        {classType} MyClass
                                        {{                                           
                                            public void Foo() {{ }}
                                        }}
                                    }}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = classTypes[0];

            Assert.Equal("App.MyClass", classModel.Name);
            Assert.Equal(1, classModel.BaseTypes.Count);

            Assert.Equal(baseType, classModel.BaseTypes[0].Type.Name);
            Assert.Equal("class", classModel.BaseTypes[0].Kind);
        }
        
        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/SimpleRecord.txt")]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/SimpleRecord2.txt")]
        public void Extract_ShouldHaveBaseClass_WhenProvidedWithRecord(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = classTypes[0];

            Assert.Equal(2, classModel.BaseTypes.Count);
            
            Assert.Equal("App.MyClass", classModel.Name);
            Assert.Equal("object", classModel.BaseTypes[0].Type.Name);
            Assert.Equal("class", classModel.BaseTypes[0].Kind);
            
            Assert.Equal("System.IEquatable<App.MyClass>", classModel.BaseTypes[1].Type.Name);
            Assert.Equal("interface", classModel.BaseTypes[1].Kind);
        }
        
        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/SimpleInterface.txt")]
        public void Extract_ShouldHaveNoBaseClass_WhenProvidedWithInterface(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = classTypes[0];

            Assert.Equal("App.MyClass", classModel.Name);
            Assert.Equal("App.MyClass", classModel.Name);
            Assert.Empty(classModel.BaseTypes);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/SimpleEnum.txt")]
        public void Extract_ShouldHaveEnumBaseClass_WhenProvidedWithEnum(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = classTypes[0];

            Assert.Equal("App.MyClass", classModel.Name);
            Assert.Equal(1, classModel.BaseTypes.Count);
            Assert.Equal("System.Enum", classModel.BaseTypes[0].Type.Name);
            Assert.Equal("class", classModel.BaseTypes[0].Kind);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/ClassThatExtendsExternClasses.txt")]
        public void Extract_ShouldHaveBaseClassIMetric_WhenClassExtendsIMetricInterface(string fileContent)
        {
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

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/ClassThatExtendsOtherClass.txt")]
        public void Extract_ShouldHaveBaseObjectAndNoInterfaces_WhenClassOnlyExtendsOtherClass(string fileContent)
        {
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

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/BaseClassInfo/ClassThatExtendsOtherClassAndExternInterfaces.txt")]
        public void
            Extract_ShouldHaveBaseObjectAndInterfaces_WhenClassExtendsOtherClassAndImplementsMultipleInterfaces(string fileContent)
        {
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
