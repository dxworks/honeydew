using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Method.LocalVariables;

public class VisualBasicMethodLocalVariablesTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicMethodLocalVariablesTests()
    {
        var localVariablesTypeSetterVisitor = new VisualBasicLocalVariablesTypeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ILocalVariableType>>
            {
                new LocalVariableInfoVisitor()
            });

        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new VisualBasicMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            localVariablesTypeSetterVisitor
                        }),
                    })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodWithPrimitiveLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Single(classModel.Methods);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(4, methodType.LocalVariableTypes.Count);
            Assert.Equal("sum", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("Integer", methodType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("min", methodType.LocalVariableTypes[1].Name);
            Assert.Equal("Integer", methodType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("max", methodType.LocalVariableTypes[2].Name);
            Assert.Equal("Integer", methodType.LocalVariableTypes[2].Type.Name);
            Assert.Equal("s", methodType.LocalVariableTypes[3].Name);
            Assert.Equal("String", methodType.LocalVariableTypes[3].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithCustomClassLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Methods.Count);

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
    }

    [Theory]
    [FilePath("TestData/MethodWithLocalVariableFromTypeof.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithTypeofSyntax(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Single(classModel.Methods);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.LocalVariableTypes.Count);
            Assert.Equal("type", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("System.Type", methodType.LocalVariableTypes[0].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithLocalVariableFromNameof.txt")]
    [FilePath("TestData/MethodWithLocalVariableFromNameofOfEnum.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithNameofSyntax(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.LocalVariableTypes.Count);
            Assert.Equal("type", methodType.LocalVariableTypes[0].Name);
            Assert.Equal("String", methodType.LocalVariableTypes[0].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithExternClassLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Methods.Count);

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
    [FilePath("TestData/MethodWithArrayLocalVariable.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(3, methodType.LocalVariableTypes.Count);
            Assert.Equal("Integer()", methodType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2()", methodType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass()", methodType.LocalVariableTypes[2].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithLocalVariableFromForeach.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalVariablesFromForeach(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(3, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.LocalVariableTypes.Count);
        }

        Assert.Equal("Namespace1.Class2", classModel.Methods[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("Integer", classModel.Methods[1].LocalVariableTypes[0].Type.Name);
        Assert.Equal("ExternClass", classModel.Methods[2].LocalVariableTypes[0].Type.Name);
    }

    [Theory]
    [FilePath("TestData/ClassWithNonPrimitiveLocalVariablesOfPropertyFromOtherClass.txt")]
    [FilePath("TestData/ClassWithNonPrimitiveLocalVariablesOfMethodFromOtherClass.txt")]
    public async Task
        Extract_ShouldHaveLocalVariablesDependencies_WhenGivenPropertyFromOtherClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        foreach (var methodType in classModel.Methods)
        {
            foreach (var localVariableType in methodType.LocalVariableTypes)
            {
                Assert.Equal("Integer", localVariableType.Type.Name);
            }
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithUnknownClassMembersInLocalVariables.txt")]
    public async Task Extract_ShouldHaveNoLocalVariablesDependencies_WhenGivenUnknownClassMembers(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(3, methodType.LocalVariableTypes.Count);
            foreach (var localVariableType in methodType.LocalVariableTypes)
            {
                Assert.Equal("Integer", localVariableType.Type.Name);
            }
        }

        _loggerMock.Verify(logger => logger.Log("Could not set 3 local variables", LogLevels.Warning), Times.Once);
    }

    [Theory]
    [FilePath("TestData/MethodWithAwaitStatement.txt")]
    public async Task Extract_ShouldHaveLocalVariableDependencies_WhenGivenMethodWithAwaitStatement(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(1, methodType.LocalVariableTypes.Count);
        foreach (var localVariableType in methodType.LocalVariableTypes)
        {
            Assert.Equal("Integer", localVariableType.Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithAwaitStatementWithUnknownClass.txt")]
    public async Task Extract_ShouldHaveLocalVariableDependencies_WhenGivenMethodWithAwaitStatementWithUnknownClass(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(1, methodType.LocalVariableTypes.Count);
        foreach (var localVariableType in methodType.LocalVariableTypes)
        {
            Assert.Equal("ExternClass", localVariableType.Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithAwaitStatementWithUnknownGenericClass.txt")]
    public async Task
        Extract_ShouldHaveLocalVariableDependencies_WhenGivenMethodWithAwaitStatementWithUnknownGenericClass(
            string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(1, methodType.LocalVariableTypes.Count);
        foreach (var localVariableType in methodType.LocalVariableTypes)
        {
            Assert.Equal("ExternClass(Of Integer, ExternClass(Of Double))", localVariableType.Type.Name);
        }
    }
}
