using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Constructor
{
    public class CSharpConstructorLocalVariablesTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpConstructorLocalVariablesTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
                    {
                        new LocalVariableInfoVisitor()
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/LocalVariables/ConstructorWithPrimitiveLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Constructors.Count);

            foreach (var constructorType in classModel.Constructors)
            {
                Assert.Equal(4, constructorType.LocalVariableTypes.Count);
                Assert.Equal("int", constructorType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("int", constructorType.LocalVariableTypes[1].Type.Name);
                Assert.Equal("int", constructorType.LocalVariableTypes[2].Type.Name);
                Assert.Equal("string", constructorType.LocalVariableTypes[3].Type.Name);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/LocalVariables/ConstructorWithCustomClassLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Constructors.Count);

            foreach (var constructorType in classModel.Constructors)
            {
                Assert.Equal(5, constructorType.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Parent", constructorType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2", constructorType.LocalVariableTypes[3].Type.Name);
                Assert.Equal("Namespace1.Class3", constructorType.LocalVariableTypes[4].Type.Name);
            }

            Assert.Equal("Namespace1.Parent", classModel.Constructors[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Parent", classModel.Constructors[0].LocalVariableTypes[2].Type.Name);

            Assert.Equal("Namespace1.Class2", classModel.Constructors[1].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Class3", classModel.Constructors[1].LocalVariableTypes[2].Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/LocalVariables/ConstructorWithExternClassLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Constructors.Count);

            foreach (var constructorType in classModel.Constructors)
            {
                Assert.Equal(3, constructorType.LocalVariableTypes.Count);
                foreach (var localVariableType in constructorType.LocalVariableTypes)
                {
                    Assert.Equal("ExternClass", localVariableType.Type.Name);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/LocalVariables/ConstructorWithArrayLocalVariable.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Constructors.Count);

            foreach (var constructorType in classModel.Constructors)
            {
                Assert.Equal(3, constructorType.LocalVariableTypes.Count);
                Assert.Equal("int[]", constructorType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2[]", constructorType.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass[]", constructorType.LocalVariableTypes[2].Type.Name);
            }
        }
    }
}
