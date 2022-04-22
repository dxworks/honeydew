using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.LocalVariables;

public class CSharpMethodLocalVariablesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpMethodLocalVariablesTests()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(_loggerMock.Object,
            new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new MethodSetterClassVisitor(_loggerMock.Object, new List<ICSharpMethodVisitor>
            {
                new MethodInfoVisitor(),
                localVariablesTypeSetterVisitor
            }),
        }));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/MethodWithPrimitiveLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(4, methodType.LocalVariableTypes.Count);
            Assert.Equal("sum", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("int", methodType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("min", methodType.LocalVariableTypes[1].Name);
            Assert.Equal("int", methodType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("max", methodType.LocalVariableTypes[2].Name);
            Assert.Equal("int", methodType.LocalVariableTypes[2].Type.Name);
            Assert.Equal("s", methodType.LocalVariableTypes[3].Name);
            Assert.Equal("string", methodType.LocalVariableTypes[3].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithCustomClassLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(5, methodType.LocalVariableTypes.Count);
            Assert.Equal("parent", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("Namespace1.Parent", methodType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("class3", methodType.LocalVariableTypes[3].Name);
            Assert.Equal("Namespace1.Class2", methodType.LocalVariableTypes[3].Type.Name);
            Assert.Equal("class4", methodType.LocalVariableTypes[4].Name);
            Assert.Equal("Namespace1.Class3", methodType.LocalVariableTypes[4].Type.Name);
        }

        Assert.Equal("class1", classModel.Methods[0].LocalVariableTypes[1].Name);
        Assert.Equal("Namespace1.Parent", classModel.Methods[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("class2", classModel.Methods[0].LocalVariableTypes[2].Name);
        Assert.Equal("Namespace1.Parent", classModel.Methods[0].LocalVariableTypes[2].Type.Name);

        Assert.Equal("class1", classModel.Methods[1].LocalVariableTypes[1].Name);
        Assert.Equal("Namespace1.Class2", classModel.Methods[1].LocalVariableTypes[1].Type.Name);
        Assert.Equal("class2", classModel.Methods[1].LocalVariableTypes[2].Name);
        Assert.Equal("Namespace1.Class3", classModel.Methods[1].LocalVariableTypes[2].Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodWithLocalVariableFromTypeof.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithTypeofSyntax(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.LocalVariableTypes.Count);
            Assert.Equal("type", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("System.Type", methodType.LocalVariableTypes[0].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithLocalVariableFromNameof.txt")]
    [FileData("TestData/MethodWithLocalVariableFromNameofOfEnum.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithNameofSyntax(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.LocalVariableTypes.Count);
            Assert.Equal("type", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("string", methodType.LocalVariableTypes[0].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithExternClassLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(3, methodType.LocalVariableTypes.Count);
            foreach (var localVariableType in methodType.LocalVariableTypes)
            {
                Assert.Equal("ExternClass", localVariableType.Type.Name);
            }
        }
    }

    [Theory]
    [FileData("TestData/MethodWithArrayLocalVariable.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(3, methodType.LocalVariableTypes.Count);
            Assert.Equal("int[]", methodType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2[]", methodType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass[]", methodType.LocalVariableTypes[2].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithLocalVariableFromIfAndSwitch.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalVariablesFromIfAndSwitch(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(2, methodType.LocalVariableTypes.Count);
            Assert.Equal("Namespace1.Class2", methodType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class3", methodType.LocalVariableTypes[1].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithLocalVariableFromForeach.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalVariablesFromForeach(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(3, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(2, methodType.LocalVariableTypes.Count);
        }

        Assert.Equal("Namespace1.Class2", classModel.Methods[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("Namespace1.Class2", classModel.Methods[0].LocalVariableTypes[1].Type.Name);

        Assert.Equal("int", classModel.Methods[1].LocalVariableTypes[0].Type.Name);
        Assert.Equal("int", classModel.Methods[1].LocalVariableTypes[1].Type.Name);

        Assert.Equal("ExternClass", classModel.Methods[2].LocalVariableTypes[0].Type.Name);
        Assert.Equal("ExternClass", classModel.Methods[2].LocalVariableTypes[1].Type.Name);
    }

    [Theory]
    [FileData("TestData/ClassWithNonPrimitiveLocalVariablesOfPropertyFromOtherClass.txt")]
    [FileData("TestData/ClassWithNonPrimitiveLocalVariablesOfMethodFromOtherClass.txt")]
    public void
        Extract_ShouldHaveLocalVariablesDependencies_WhenGivenPropertyFromOtherClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        foreach (var methodType in classModel.Methods)
        {
            foreach (var localVariableType in methodType.LocalVariableTypes)
            {
                Assert.Equal("int", localVariableType.Type.Name);
            }
        }
    }

    [Theory]
    [FileData("TestData/MethodWithUnknownClassMembersInLocalVariables.txt")]
    public void Extract_ShouldHaveNoLocalVariablesDependencies_WhenGivenUnknownClassMembers(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(3, methodType.LocalVariableTypes.Count);
            foreach (var localVariableType in methodType.LocalVariableTypes)
            {
                Assert.Equal("int", localVariableType.Type.Name);
            }
        }

        _loggerMock.Verify(logger => logger.Log("Could not set 3 local variables", LogLevels.Warning), Times.Once);
    }

    [Theory]
    [FileData("TestData/MethodWithAwaitStatement.txt")]
    public void Extract_ShouldHaveLocalVariableDependencies_WhenGivenMethodWithAwaitStatement(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(2, methodType.LocalVariableTypes.Count);
        foreach (var localVariableType in methodType.LocalVariableTypes)
        {
            Assert.Equal("int", localVariableType.Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithAwaitStatementWithUnknownClass.txt")]
    public void Extract_ShouldHaveLocalVariableDependencies_WhenGivenMethodWithAwaitStatementWithUnknownClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(2, methodType.LocalVariableTypes.Count);
        foreach (var localVariableType in methodType.LocalVariableTypes)
        {
            Assert.Equal("ExternClass", localVariableType.Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithAwaitStatementWithUnknownGenericClass.txt")]
    public void Extract_ShouldHaveLocalVariableDependencies_WhenGivenMethodWithAwaitStatementWithUnknownGenericClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(2, methodType.LocalVariableTypes.Count);
        foreach (var localVariableType in methodType.LocalVariableTypes)
        {
            Assert.Equal("ExternClass<int, ExternClass<double>>", localVariableType.Type.Name);
        }
    }
}
