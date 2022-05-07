using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Property.Attributes;

public class VisualBasicPropertyAttributeTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicPropertyAttributeTests()
    {
        var attributeSetterVisitor = new VisualBasicAttributeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IAttributeType>>
            {
                new AttributeInfoVisitor()
            });

        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new VisualBasicPropertySetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            attributeSetterVisitor,
                            new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    new MethodInfoVisitor(),
                                    attributeSetterVisitor,
                                    new VisualBasicReturnValueSetterVisitor(_loggerMock.Object,
                                        new List<ITypeVisitor<IReturnValueType>>
                                        {
                                            new ReturnValueInfoVisitor(),
                                            attributeSetterVisitor
                                        })
                                })
                        })
                    })
            });


        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/PropertyWithOneAttributeWithNoParams.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            var attributeTypes = property.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("property", attributeType.Target);
                Assert.Equal("System.SerializableAttribute", attributeType.Name);
                Assert.Empty(attributeType.ParameterTypes);
            }
        }
    }

    [Theory]
    [FilePath("TestData/PropertyWithOneAttributeWithOneParam.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classModel.Properties.Count);

        foreach (var propertyType in classModel.Properties)
        {
            var attributeTypes = propertyType.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("property", attributeType.Target);
                Assert.Equal("Obsolete", attributeType.Name);
                Assert.Equal(1, attributeType.ParameterTypes.Count);
                Assert.Equal("System.String", attributeType.ParameterTypes[0].Type.Name);
                Assert.Equal("System.String", attributeType.ParameterTypes[0].Type.FullType.Name);
                Assert.False(attributeType.ParameterTypes[0].Type.FullType.IsNullable);
            }
        }
    }

    [Theory]
    [FilePath("TestData/PropertyWithMultipleAttributesWithMultipleParams.txt")]
    [FilePath("TestData/PropertyWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var properties = ((VisualBasicClassModel)classTypes[0]).Properties;

        Assert.Equal(1, properties.Count);

        foreach (var propertyType in properties)
        {
            var attributeTypes = propertyType.Attributes;
            Assert.Equal(3, attributeTypes.Count);
            foreach (var attribute in attributeTypes)
            {
                Assert.Equal("property", attribute.Target);
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
    [FilePath("TestData/PropertyWithCustomAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[1];

        Assert.Equal(1, classType.Properties.Count);

        foreach (var propertyType in classType.Properties)
        {
            var fieldAttributes = propertyType.Attributes;
            Assert.Equal(4, fieldAttributes.Count);
            foreach (var attribute in fieldAttributes)
            {
                Assert.Equal("property", attribute.Target);
                Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
            }

            var attribute1 = fieldAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = fieldAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = fieldAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("String", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = fieldAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);
        }
    }

    [Theory]
    [FilePath("TestData/PropertyWithExternAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classType.Properties.Count);

        foreach (var propertyType in classType.Properties)
        {
            Assert.Equal(5, propertyType.Attributes.Count);
            foreach (var attribute in propertyType.Attributes)
            {
                Assert.Equal("property", attribute.Target);
            }

            var attribute1 = propertyType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = propertyType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = propertyType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = propertyType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = propertyType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }

    [Theory]
    [FilePath("TestData/PropertyWithAccessorsWithAttributes.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithPropertyAccessors(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(4, classType.Properties.Count);
        foreach (var propertyType in classType.Properties)
        {
            Assert.Empty(propertyType.Attributes);

            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(3, accessor.Attributes.Count);

                foreach (var accessorAttribute in accessor.Attributes)
                {
                    Assert.Equal("method", accessorAttribute.Target);
                }

                var attribute1 = accessor.Attributes[0];
                Assert.Equal("Namespace1.MyAttribute", attribute1.Name);
                Assert.Equal(1, attribute1.ParameterTypes.Count);
                Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);

                var attribute2 = accessor.Attributes[1];
                Assert.Equal("System.ObsoleteAttribute", attribute2.Name);
                Assert.Empty(attribute2.ParameterTypes);

                var attribute3 = accessor.Attributes[2];
                Assert.Equal("ExternAttribute", attribute3.Name);
                Assert.Empty(attribute3.ParameterTypes);
            }
        }
    }
}
