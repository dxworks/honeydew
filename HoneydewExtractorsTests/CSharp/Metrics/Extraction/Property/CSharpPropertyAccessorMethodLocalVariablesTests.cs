using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Property
{
    public class CSharpPropertyAccessorMethodLocalVariablesTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpPropertyAccessorMethodLocalVariablesTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new PropertySetterClassVisitor(new List<IPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                    {
                        new MethodInfoVisitor(),
                        new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
                        {
                            new LocalVariableInfoVisitor()
                        })
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Property/LocalVariables/MethodAccessorWithPrimitiveLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Properties.Count);

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    Assert.Equal(4, accessor.LocalVariableTypes.Count);
                    Assert.Equal("int", accessor.LocalVariableTypes[0].Type.Name);
                    Assert.Equal("int", accessor.LocalVariableTypes[1].Type.Name);
                    Assert.Equal("int", accessor.LocalVariableTypes[2].Type.Name);
                    Assert.Equal("string", accessor.LocalVariableTypes[3].Type.Name);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Property/LocalVariables/MethodAccessorWithCustomClassLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Properties.Count);

            foreach (var property in classModel.Properties)
            {
                foreach (var accessor in property.Accessors)
                {
                    Assert.Equal(5, accessor.LocalVariableTypes.Count);
                    Assert.Equal("Namespace1.Parent", accessor.LocalVariableTypes[0].Type.Name);
                    Assert.Equal("Namespace1.Class2", accessor.LocalVariableTypes[3].Type.Name);
                    Assert.Equal("Namespace1.Class3", accessor.LocalVariableTypes[4].Type.Name);
                }
            }


            Assert.Equal("Namespace1.Parent", classModel.Properties[0].Accessors[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Parent", classModel.Properties[0].Accessors[0].LocalVariableTypes[2].Type.Name);
            Assert.Equal("Namespace1.Class2", classModel.Properties[0].Accessors[1].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Class3", classModel.Properties[0].Accessors[1].LocalVariableTypes[2].Type.Name);

            Assert.Equal("Namespace1.Parent", classModel.Properties[1].Accessors[1].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Parent", classModel.Properties[1].Accessors[1].LocalVariableTypes[2].Type.Name);
            Assert.Equal("Namespace1.Class2", classModel.Properties[1].Accessors[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Class3", classModel.Properties[1].Accessors[0].LocalVariableTypes[2].Type.Name);

            Assert.Equal("Namespace1.Parent", classModel.Properties[2].Accessors[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Parent", classModel.Properties[2].Accessors[0].LocalVariableTypes[2].Type.Name);
            Assert.Equal("Namespace1.Class2", classModel.Properties[2].Accessors[1].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Class3", classModel.Properties[2].Accessors[1].LocalVariableTypes[2].Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Property/LocalVariables/MethodAccessorWithExternClassLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;
            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(3, classModel.Properties.Count);

            foreach (var property in classModel.Properties)
            {
                foreach (var accessor in property.Accessors)
                {
                    Assert.Equal(3, accessor.LocalVariableTypes.Count);
                    foreach (var localVariableType in accessor.LocalVariableTypes)
                    {
                        Assert.Equal("ExternClass", localVariableType.Type.Name);
                    }
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Property/LocalVariables/MethodAccessorWithArrayLocalVariable.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Properties.Count);

            foreach (var property in classModel.Properties)
            {
                foreach (var accessor in property.Accessors)
                {
                    Assert.Equal(3, accessor.LocalVariableTypes.Count);
                    Assert.Equal("int[]", accessor.LocalVariableTypes[0].Type.Name);
                    Assert.Equal("Namespace1.Class2[]", accessor.LocalVariableTypes[1].Type.Name);
                    Assert.Equal("ExternClass[]", accessor.LocalVariableTypes[2].Type.Name);
                }
            }
        }
    }
}
