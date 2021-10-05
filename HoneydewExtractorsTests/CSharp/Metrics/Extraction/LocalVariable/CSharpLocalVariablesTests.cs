using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.LocalVariable
{
    public class CSharpLocalVariablesTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpLocalVariablesTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    localVariablesTypeSetterVisitor
                }),
                new ConstructorSetterClassVisitor(new List<IConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    localVariablesTypeSetterVisitor
                }),
                new PropertySetterClassVisitor(new List<IPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                    {
                        new MethodInfoVisitor(),
                        localVariablesTypeSetterVisitor
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/LocalVariable/MethodWithRefLocals.txt")]
        public void Extract_ShouldHaveRefModifier_WhenGivenMethodWithLocalVariables(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var methodModel in classModel.Methods)
            {
                foreach (var localVariableType in methodModel.LocalVariableTypes)
                {
                    Assert.Equal("ref", localVariableType.Modifier);
                }
            }
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/LocalVariable/MethodWithRefReadonlyLocals.txt")]
        public void Extract_ShouldHaveRefReadonlyModifier_WhenGivenMethodWithLocalVariables(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var methodModel in classModel.Methods)
            {
                foreach (var localVariableType in methodModel.LocalVariableTypes)
                {
                    Assert.Equal("ref readonly", localVariableType.Modifier);
                }
            }
        }
        
        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/LocalVariable/ConstructorWithRefLocals.txt")]
        public void Extract_ShouldHaveRefModifier_WhenGivenConstructorWithLocalVariables(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var constructorType in classModel.Constructors)
            {
                foreach (var localVariableType in constructorType.LocalVariableTypes)
                {
                    Assert.Equal("ref", localVariableType.Modifier);
                }
            }
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/LocalVariable/ConstructorWithRefReadonlyLocals.txt")]
        public void Extract_ShouldHaveRefReadonlyModifier_WhenGivenConstructorWithLocalVariables(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var constructorType in classModel.Constructors)
            {
                foreach (var localVariableType in constructorType.LocalVariableTypes)
                {
                    Assert.Equal("ref readonly", localVariableType.Modifier);
                }
            }
        }
        
        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/LocalVariable/PropertyWithRefLocals.txt")]
        public void Extract_ShouldHaveRefModifier_WhenGivenPropertyWithLocalVariables(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var localVariableType in accessor.LocalVariableTypes)
                    {
                        Assert.Equal("ref", localVariableType.Modifier);
                    }   
                }
            }
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/LocalVariable/PropertyWithRefReadonlyLocals.txt")]
        public void Extract_ShouldHaveRefReadonlyModifier_WhenGivenPropertyWithLocalVariables(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var localVariableType in accessor.LocalVariableTypes)
                    {
                        Assert.Equal("ref readonly", localVariableType.Modifier);
                    }   
                }
            }
        }
    }
}
