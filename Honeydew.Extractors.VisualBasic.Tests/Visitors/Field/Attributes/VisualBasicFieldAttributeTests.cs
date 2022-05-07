using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Field.Attributes;

public class VisualBasicFieldAttributeTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicFieldAttributeTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new VisualBasicFieldSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IFieldType>>
                        {
                            new FieldInfoVisitor(),
                            new VisualBasicAttributeSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAttributeType>>
                            {
                                new AttributeInfoVisitor()
                            })
                        })
                    })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }



    [Theory]
    [FilePath("TestData/FieldWithOneAttributeWithNoParams.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Fields.Count);

        foreach (var fieldType in classModel.Fields)
        {
            var attributeTypes = fieldType.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("field", attributeType.Target);
                Assert.Equal("System.SerializableAttribute", attributeType.Name);
                Assert.Empty(attributeType.ParameterTypes);
            }
        }
    }

    [Theory]
    [FilePath("TestData/FieldWithOneAttributeWithOneParam.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classModel.Fields.Count);

        foreach (var fieldType in classModel.Fields)
        {
            var attributeTypes = fieldType.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("field", attributeType.Target);
                Assert.Equal("Obsolete", attributeType.Name);
                Assert.Equal(1, attributeType.ParameterTypes.Count);
                Assert.Equal("System.String", attributeType.ParameterTypes[0].Type.Name);
                Assert.Equal("System.String", attributeType.ParameterTypes[0].Type.FullType.Name);
                Assert.False(attributeType.ParameterTypes[0].Type.FullType.IsNullable);
            }
        }
    }

    [Theory]
    [FilePath("TestData/FieldWithMultipleAttributesWithMultipleParams.txt")]
    [FilePath("TestData/FieldWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var fields = ((VisualBasicClassModel)classTypes[0]).Fields;

        Assert.Equal(1, fields.Count);

        foreach (var fieldType in fields)
        {
            var attributeTypes = fieldType.Attributes;
            Assert.Equal(3, attributeTypes.Count);
            foreach (var attribute in attributeTypes)
            {
                Assert.Equal("field", attribute.Target);
            }

            var attribute1 = attributeTypes[0];
            Assert.Equal(2, attribute1.ParameterTypes.Count);
            Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.FullType.Name);
            Assert.False(attribute1.ParameterTypes[0].Type.FullType.IsNullable);
            Assert.Equal("Boolean", attribute1.ParameterTypes[1].Type.Name);
            Assert.Equal("Boolean", attribute1.ParameterTypes[1].Type.FullType.Name);
            Assert.False(attribute1.ParameterTypes[1].Type.FullType.IsNullable);

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
    [FilePath("TestData/FieldWithCustomAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[1];

        Assert.Equal(1, classType.Fields.Count);

        foreach (var fieldType in classType.Fields)
        {
            var fieldAttributes = fieldType.Attributes;
            Assert.Equal(4, fieldAttributes.Count);
            foreach (var attribute in fieldAttributes)
            {
                Assert.Equal("field", attribute.Target);
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
    [FilePath("TestData/FieldWithExternAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classType.Fields.Count);

        foreach (var fieldType in classType.Fields)
        {
            Assert.Equal(5, fieldType.Attributes.Count);
            foreach (var attribute in fieldType.Attributes)
            {
                Assert.Equal("field", attribute.Target);
            }

            var attribute1 = fieldType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = fieldType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = fieldType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = fieldType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = fieldType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }
}
