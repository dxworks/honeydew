using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Delegate.Attributes;

public class VisualBasicDelegateAttributeTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicDelegateAttributeTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicDelegateSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    new BaseInfoDelegateVisitor(),
                    new VisualBasicAttributeSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAttributeType>>
                    {
                        new AttributeInfoVisitor()
                    })
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/DelegateWithOneAttributeWithNoParams.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[0].Attributes.Count);
        Assert.Equal("type", classTypes[0].Attributes[0].Target);
        Assert.Equal("System.SerializableAttribute", classTypes[0].Attributes[0].Name);
        Assert.Empty(classTypes[0].Attributes[0].ParameterTypes);
    }

    [Theory]
    [FilePath("TestData/DelegateWithOneAttributeWithOneParam.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[0].Attributes.Count);
        Assert.Equal("type", classTypes[0].Attributes[0].Target);
        Assert.Equal("Obsolete", classTypes[0].Attributes[0].Name);
        Assert.Equal(1, classTypes[0].Attributes[0].ParameterTypes.Count);
        Assert.Equal("System.String", classTypes[0].Attributes[0].ParameterTypes[0].Type.Name);
        Assert.Equal("System.String", classTypes[0].Attributes[0].ParameterTypes[0].Type.FullType.Name);
        Assert.False(classTypes[0].Attributes[0].ParameterTypes[0].Type.FullType.IsNullable);
    }

    [Theory]
    [FilePath("TestData/DelegateWithMultipleAttributesWithMultipleParams.txt")]
    [FilePath("TestData/DelegateWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(3, classTypes[0].Attributes.Count);
        foreach (var attribute in classTypes[0].Attributes)
        {
            Assert.Equal("type", attribute.Target);
        }

        var attribute1 = classTypes[0].Attributes[0];
        Assert.Equal(2, attribute1.ParameterTypes.Count);
        Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
        Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);
        Assert.Equal("String", attribute1.ParameterTypes[0].Type.FullType.Name);
        Assert.False(attribute1.ParameterTypes[0].Type.FullType.IsNullable);
        Assert.Equal("Boolean", attribute1.ParameterTypes[1].Type.Name);

        var attribute2 = classTypes[0].Attributes[1];
        Assert.Equal("System.SerializableAttribute", attribute2.Name);
        Assert.Empty(attribute2.ParameterTypes);

        var attribute3 = classTypes[0].Attributes[2];
        Assert.Equal("System.AttributeUsageAttribute", attribute3.Name);
        Assert.Equal(1, attribute3.ParameterTypes.Count);
        Assert.Equal("System.AttributeTargets", attribute3.ParameterTypes[0].Type.Name);
    }

    [Theory]
    [FilePath("TestData/DelegateWithCustomAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = classTypes[0];

        Assert.Equal(4, classType.Attributes.Count);
        foreach (var attribute in classType.Attributes)
        {
            Assert.Equal("type", attribute.Target);
            Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
        }

        var attribute1 = classType.Attributes[0];
        Assert.Equal(1, attribute1.ParameterTypes.Count);
        Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);

        var attribute2 = classType.Attributes[1];
        Assert.Empty(attribute2.ParameterTypes);

        var attribute3 = classType.Attributes[2];
        Assert.Equal(1, attribute3.ParameterTypes.Count);
        Assert.Equal("String", attribute3.ParameterTypes[0].Type.Name);

        var attribute4 = classType.Attributes[3];
        Assert.Empty(attribute4.ParameterTypes);
    }

    [Theory]
    [FilePath("TestData/DelegateWithExternAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = classTypes[0];

        Assert.Equal(5, classType.Attributes.Count);
        foreach (var attribute in classType.Attributes)
        {
            Assert.Equal("type", attribute.Target);
        }

        var attribute1 = classType.Attributes[0];
        Assert.Equal("Extern", attribute1.Name);
        Assert.Equal(1, attribute1.ParameterTypes.Count);
        Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

        var attribute2 = classType.Attributes[1];
        Assert.Equal("ExternAttribute", attribute2.Name);
        Assert.Empty(attribute2.ParameterTypes);

        var attribute3 = classType.Attributes[2];
        Assert.Equal("ExternAttribute", attribute3.Name);
        Assert.Equal(2, attribute3.ParameterTypes.Count);
        Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
        Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

        var attribute4 = classType.Attributes[3];
        Assert.Equal("Extern", attribute4.Name);
        Assert.Equal(1, attribute4.ParameterTypes.Count);
        Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

        var attribute5 = classType.Attributes[4];
        Assert.Equal("Extern", attribute5.Name);
        Assert.Equal(1, attribute5.ParameterTypes.Count);
        Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
    }
}
