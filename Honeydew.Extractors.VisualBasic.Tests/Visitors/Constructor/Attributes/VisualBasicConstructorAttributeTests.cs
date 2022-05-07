using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Constructor.Attributes;

public class VisualBasicConstructorAttributeTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicConstructorAttributeTests()
    {
        var visualBasicAttributeSetterVisitor = new VisualBasicAttributeSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAttributeType>>
        {
            new AttributeInfoVisitor()
        });
        var visualBasicConstructorSetterVisitor = new VisualBasicConstructorSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IConstructorType>>
        {
            new ConstructorInfoVisitor(),
            visualBasicAttributeSetterVisitor
        });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    visualBasicConstructorSetterVisitor
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    visualBasicConstructorSetterVisitor
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ConstructorWithOneAttributeWithNoParams.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);

        var attributeTypes = classModel.Constructors[0].Attributes;
        Assert.Equal(1, attributeTypes.Count);
        Assert.Equal("method", attributeTypes[0].Target);
        Assert.Equal("System.SerializableAttribute", attributeTypes[0].Name);
        Assert.Empty(attributeTypes[0].ParameterTypes);
    }

    [Theory]
    [FilePath("TestData/ConstructorWithOneAttributeWithOneParam.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classModel.Constructors.Count);

        var attributeTypes = classModel.Constructors[0].Attributes;
        Assert.Equal(1, attributeTypes.Count);
        Assert.Equal("method", attributeTypes[0].Target);
        Assert.Equal("Obsolete", attributeTypes[0].Name);
        Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
        Assert.Equal("System.String", attributeTypes[0].ParameterTypes[0].Type.Name);
        Assert.Equal("System.String", attributeTypes[0].ParameterTypes[0].Type.FullType.Name);
        Assert.False(attributeTypes[0].ParameterTypes[0].Type.FullType.IsNullable);
    }

    [Theory]
    [FilePath("TestData/ConstructorWithMultipleAttributesWithMultipleParams.txt")]
    [FilePath("TestData/ConstructorWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var constructors = ((VisualBasicClassModel)classTypes[0]).Constructors;

        Assert.Equal(1, constructors.Count);

        foreach (var constructorType in constructors)
        {
            var attributeTypes = constructorType.Attributes;
            Assert.Equal(3, attributeTypes.Count);
            foreach (var attribute in attributeTypes)
            {
                Assert.Equal("method", attribute.Target);
            }

            var attribute1 = attributeTypes[0];
            Assert.Equal(2, attribute1.ParameterTypes.Count);
            Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.FullType.Name);
            Assert.False(attribute1.ParameterTypes[0].Type.FullType.IsNullable);
            Assert.Equal("Boolean", attribute1.ParameterTypes[1].Type.Name);

            var attribute2 = attributeTypes[1];
            Assert.Equal("System.SerializableAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = attributeTypes[2];
            Assert.Equal("System.AttributeUsageAttribute", attribute3.Name);
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("System.AttributeTargets", attribute3.ParameterTypes[0].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/ConstructorWithCustomAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[1];

        Assert.Equal(1, classType.Constructors.Count);

        foreach (var constructorType in classType.Constructors)
        {
            var constructorTypeAttributes = constructorType.Attributes;
            Assert.Equal(4, constructorTypeAttributes.Count);
            foreach (var attribute in constructorTypeAttributes)
            {
                Assert.Equal("method", attribute.Target);
                Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
            }

            var attribute1 = constructorTypeAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = constructorTypeAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = constructorTypeAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("String", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = constructorTypeAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);
        }
    }

    [Theory]
    [FilePath("TestData/ConstructorWithExternAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classType.Constructors.Count);

        foreach (var constructorType in classType.Constructors)
        {
            Assert.Equal(5, constructorType.Attributes.Count);
            foreach (var attribute in constructorType.Attributes)
            {
                Assert.Equal("method", attribute.Target);
            }

            var attribute1 = constructorType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = constructorType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = constructorType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = constructorType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = constructorType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }
}
