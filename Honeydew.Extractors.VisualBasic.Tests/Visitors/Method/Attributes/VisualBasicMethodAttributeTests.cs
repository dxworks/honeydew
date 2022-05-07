using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Method.Attributes;

public class VisualBasicMethodAttributeTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicMethodAttributeTests()
    {
        var compositeVisitor =
            new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
                new List<ITypeVisitor<ICompilationUnitType>>
                {
                    new VisualBasicClassSetterVisitor(_loggerMock.Object,
                        new List<ITypeVisitor<IMembersClassType>>
                        {
                            new BaseInfoClassVisitor(),
                            new VisualBasicMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                            {
                                new MethodInfoVisitor(),
                                new VisualBasicAttributeSetterVisitor(_loggerMock.Object,
                                    new List<ITypeVisitor<IAttributeType>>
                                    {
                                        new AttributeInfoVisitor()
                                    })
                            })
                        })
                });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodWithOneAttributeWithNoParams.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            var attributeTypes = methodType.Attributes;
            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("method", attributeTypes[0].Target);
            Assert.Equal("System.SerializableAttribute", attributeTypes[0].Name);
            Assert.Empty(attributeTypes[0].ParameterTypes);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithOneAttributeWithOneParam.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            var attributeTypes = methodType.Attributes;
            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("method", attributeTypes[0].Target);
            Assert.Equal("Obsolete", attributeTypes[0].Name);
            Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
            Assert.Equal("System.String", attributeTypes[0].ParameterTypes[0].Type.Name);
            Assert.Equal("System.String", attributeTypes[0].ParameterTypes[0].Type.FullType.Name);
            Assert.False(attributeTypes[0].ParameterTypes[0].Type.FullType.IsNullable);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithMultipleAttributesWithMultipleParams.txt")]
    [FilePath("TestData/MethodWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var methods = ((VisualBasicClassModel)classTypes[0]).Methods;

        Assert.Equal(2, methods.Count);

        foreach (var methodType in methods)
        {
            var attributeTypes = methodType.Attributes;
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
    [FilePath("TestData/MethodWithCustomAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[1];

        Assert.Equal(2, classType.Methods.Count);

        foreach (var methodType in classType.Methods)
        {
            var methodTypeAttributes = methodType.Attributes;
            Assert.Equal(4, methodTypeAttributes.Count);
            foreach (var attribute in methodTypeAttributes)
            {
                Assert.Equal("method", attribute.Target);
                Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
            }

            var attribute1 = methodTypeAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = methodTypeAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = methodTypeAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("String", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = methodTypeAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);
        }
    }

    [Theory]
    [FilePath("TestData/MethodWithExternAttribute.txt")]
    public async Task Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classType = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(2, classType.Methods.Count);

        foreach (var methodType in classType.Methods)
        {
            Assert.Equal(5, methodType.Attributes.Count);
            foreach (var attribute in methodType.Attributes)
            {
                Assert.Equal("method", attribute.Target);
            }

            var attribute1 = methodType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = methodType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = methodType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = methodType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = methodType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }
}
