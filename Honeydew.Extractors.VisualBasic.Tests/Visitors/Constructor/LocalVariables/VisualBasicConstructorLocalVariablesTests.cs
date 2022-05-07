using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Constructor.LocalVariables;

public class VisualBasicConstructorLocalVariablesTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicConstructorLocalVariablesTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new VisualBasicConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                new VisualBasicLocalVariablesSetterVisitor(_loggerMock.Object,
                                    new List<ITypeVisitor<ILocalVariableType>>
                                    {
                                        new LocalVariableInfoVisitor()
                                    })
                            })
                    })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ConstructorWithPrimitiveLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);

        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(4, constructorType.LocalVariableTypes.Count);
            Assert.Equal("sum", constructorType.LocalVariableTypes[0].Name);
            Assert.Equal("Integer", constructorType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("min", constructorType.LocalVariableTypes[1].Name);
            Assert.Equal("Integer", constructorType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("max", constructorType.LocalVariableTypes[2].Name);
            Assert.Equal("Integer", constructorType.LocalVariableTypes[2].Type.Name);
            Assert.Equal("s", constructorType.LocalVariableTypes[3].Name);
            Assert.Equal("String", constructorType.LocalVariableTypes[3].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/ConstructorWithCustomClassLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);

        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(5, constructorType.LocalVariableTypes.Count);
            Assert.Equal("Namespace1.Parent", constructorType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2", constructorType.LocalVariableTypes[3].Type.Name);
            Assert.Equal("Namespace1.Class3", constructorType.LocalVariableTypes[4].Type.Name);
        }

        Assert.Equal("Namespace1.Parent", classModel.Constructors[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Parent", classModel.Constructors[0].LocalVariableTypes[2].Type.Name);
    }

    [Theory]
    [FilePath("TestData/ConstructorWithExternClassLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);

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
    }

    [Theory]
    [FilePath("TestData/ConstructorWithArrayLocalVariable.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);

        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(3, constructorType.LocalVariableTypes.Count);
            Assert.Equal("Integer()", constructorType.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2()", constructorType.LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass()", constructorType.LocalVariableTypes[2].Type.Name);

            for (var i = 0; i < constructorType.LocalVariableTypes.Count; i++)
            {
                Assert.Equal($"array{(i == 0 ? "" : (i + 1).ToString())}",
                    constructorType.LocalVariableTypes[i].Name);
            }
        }
    }

}
