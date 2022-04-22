using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.LocalVariables;

public class CSharpLocalVariablesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpLocalVariablesTests()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(_loggerMock.Object,
            new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new MethodSetterClassVisitor(_loggerMock.Object, new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
                localVariablesTypeSetterVisitor
            }),
            new ConstructorSetterClassVisitor(_loggerMock.Object, new List<IConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                localVariablesTypeSetterVisitor
            }),
            new PropertySetterClassVisitor(_loggerMock.Object, new List<IPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(_loggerMock.Object, new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    localVariablesTypeSetterVisitor
                })
            })
        }));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/MethodWithRefLocals.txt")]
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
    [FileData("TestData/MethodWithRefReadonlyLocals.txt")]
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
    [FileData("TestData/EntitiesWithNullableLocalVariables.txt")]
    public void Extract_ShouldHaveNullableVariables_WhenEntitiesWithLocalVariables(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var typeWithLocalVariables = new ITypeWithLocalVariables[]
        {
            classModel.Constructors[0],
            classModel.Methods[0],
            classModel.Properties[0].Accessors[0],
            classModel.Properties[0].Accessors[1],
            classModel.Properties[1].Accessors[0],
            classModel.Properties[1].Accessors[1],
        };

        foreach (var typeWithLocalVariable in typeWithLocalVariables)
        {
            foreach (var localVariableType in typeWithLocalVariable.LocalVariableTypes)
            {
                Assert.Equal("int?", localVariableType.Type.Name);
                Assert.Equal("int", localVariableType.Type.FullType.Name);
                Assert.True(localVariableType.IsNullable);
            }
        }
    }

    [Theory]
    [FileData("TestData/EntitiesWithNullableLocalVariablesOfClassType.txt")]
    public void Extract_ShouldHaveNullableVariables_WhenEntitiesWithLocalVariablesOfClassType(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var typeWithLocalVariables = new ITypeWithLocalVariables[]
        {
            classModel.Constructors[0],
            classModel.Methods[0],
            classModel.Properties[0].Accessors[0],
            classModel.Properties[0].Accessors[1],
            classModel.Properties[1].Accessors[0],
            classModel.Properties[1].Accessors[1],
        };

        foreach (var typeWithLocalVariable in typeWithLocalVariables)
        {
            foreach (var localVariableType in typeWithLocalVariable.LocalVariableTypes)
            {
                Assert.Equal("Namespace1.MyClass2?", localVariableType.Type.Name);
                Assert.Equal("Namespace1.MyClass2", localVariableType.Type.FullType.Name);
                Assert.True(localVariableType.IsNullable);
            }
        }
    }
}
