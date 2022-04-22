using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Constructor.LocalVariables;

public class CSharpConstructorLocalVariablesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpConstructorLocalVariablesTests()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new ConstructorSetterClassVisitor(_loggerMock.Object, new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                new LocalVariablesTypeSetterVisitor(_loggerMock.Object, new List<ILocalVariablesVisitor>
                {
                    new LocalVariableInfoVisitor()
                })
            })
        }));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ConstructorWithPrimitiveLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Constructors.Count);

        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(4, constructorType.LocalVariableTypes.Count);
            Assert.Equal("sum", constructorType.LocalVariableTypes[0].Name);
            Assert.Equal("int", constructorType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("min", constructorType.LocalVariableTypes[1].Name);
            Assert.Equal("int", constructorType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("max", constructorType.LocalVariableTypes[2].Name);
            Assert.Equal("int", constructorType.LocalVariableTypes[2].Type.Name);
            Assert.Equal("s", constructorType.LocalVariableTypes[3].Name);
            Assert.Equal("string", constructorType.LocalVariableTypes[3].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/ConstructorWithCustomClassLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
    [FileData("TestData/ConstructorWithExternClassLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

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

        Assert.Equal("c", classModel.Constructors[0].LocalVariableTypes[0].Name);
        Assert.Equal("c2", classModel.Constructors[0].LocalVariableTypes[1].Name);
        Assert.Equal("c3", classModel.Constructors[0].LocalVariableTypes[2].Name);

        Assert.Equal("c", classModel.Constructors[1].LocalVariableTypes[0].Name);
        Assert.Equal("class1", classModel.Constructors[1].LocalVariableTypes[1].Name);
        Assert.Equal("class2", classModel.Constructors[1].LocalVariableTypes[2].Name);
    }

    [Theory]
    [FileData("TestData/ConstructorWithArrayLocalVariable.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Constructors.Count);

        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(3, constructorType.LocalVariableTypes.Count);
            Assert.Equal("int[]", constructorType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2[]", constructorType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass[]", constructorType.LocalVariableTypes[2].Type.Name);

            for (var i = 0; i < constructorType.LocalVariableTypes.Count; i++)
            {
                Assert.Equal($"array{(i == 0 ? "" : (i + 1).ToString())}",
                    constructorType.LocalVariableTypes[i].Name);
            }
        }
    }

    [Theory]
    [FileData("TestData/ConstructorWithRefLocals.txt")]
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
    [FileData("TestData/ConstructorWithRefReadonlyLocals.txt")]
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
}
