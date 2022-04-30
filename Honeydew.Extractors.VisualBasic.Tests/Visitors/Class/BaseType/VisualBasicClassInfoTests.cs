using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Class.BaseType;

public class VisualBasicClassInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicClassInfoTests()
    {
        var baseInfoClassVisitor = new BaseInfoClassVisitor();
        var baseTypesInfoVisitor = new BaseTypesClassVisitor();
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor,
                    baseTypesInfoVisitor
                }),
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor,
                    baseTypesInfoVisitor
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor,
                    baseTypesInfoVisitor
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ClassWithBaseTypes.txt")]
    public async Task Extract_ShouldHaveBaseTypesInfo_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
        Assert.Equal("Y", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Equal(2, classType.BaseTypes.Count);
        Assert.Equal("class", classType.BaseTypes[0].Kind);
        Assert.Equal("X", classType.BaseTypes[0].Type.Name);
        Assert.Equal("interface", classType.BaseTypes[1].Kind);
        Assert.Equal("MyInterface", classType.BaseTypes[1].Type.Name);
    }

    [Theory]
    [FilePath("TestData/InterfaceWithBaseTypes.txt")]
    public async Task Extract_ShouldHaveBaseTypesInfo_WhenProvidedWithInterface(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[2];
        Assert.Equal("Y", classType.Name);
        Assert.Equal("interface", classType.ClassType);
        Assert.Equal(2, classType.BaseTypes.Count);
        Assert.Equal("interface", classType.BaseTypes[0].Kind);
        Assert.Equal("X", classType.BaseTypes[0].Type.Name);
        Assert.Equal("interface", classType.BaseTypes[1].Kind);
        Assert.Equal("MyInterface", classType.BaseTypes[1].Type.Name);
    }

    [Theory]
    [FilePath("TestData/StructureWithBaseTypes.txt")]
    public async Task Extract_ShouldHaveBaseTypesInfo_WhenProvidedWithStructure(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
        Assert.Equal("Y", classType.Name);
        Assert.Equal("structure", classType.ClassType);
        Assert.Single(classType.BaseTypes);
        Assert.Equal("interface", classType.BaseTypes[0].Kind);
        Assert.Equal("MyInterface", classType.BaseTypes[0].Type.Name);
    }
    
    [Theory]
    [FilePath("TestData/ClassWithNoBaseTypes.txt")]
    public async Task Extract_ShouldHaveObjectBaseType_WhenProvidedWithClassThatHasNoBaseTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Equal("X", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Single(classType.BaseTypes);
        Assert.Equal("class", classType.BaseTypes[0].Kind);
        Assert.Equal("Object", classType.BaseTypes[0].Type.Name);
    }
}
