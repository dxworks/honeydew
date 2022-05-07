using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Enum.Attributes;

public class VisualBasicEnumAttributesTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicEnumAttributesTests()
    {
        var attributeSetterVisitor = new VisualBasicAttributeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IAttributeType>>
            {
                new AttributeInfoVisitor()
            });

        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    new BaseInfoEnumVisitor(),
                    new VisualBasicEnumLabelsSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumLabelType>>
                    {
                        new BasicEnumLabelInfoVisitor(),
                        attributeSetterVisitor,
                    }),
                    attributeSetterVisitor,
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/EnumWithAttributes.txt")]
    public async Task Extract_ShouldExtractAttribute_GivenEnum(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var enumModel = (VisualBasicEnumModel)classTypes[0];
        Assert.Equal(1, enumModel.Attributes.Count);
        Assert.Equal("type", enumModel.Attributes[0].Target);
        Assert.Equal("Obsolete", enumModel.Attributes[0].Name);
        Assert.Equal(1, enumModel.Attributes[0].ParameterTypes.Count);
        Assert.Equal("System.String", enumModel.Attributes[0].ParameterTypes[0].Type.Name);
        Assert.Equal("System.String", enumModel.Attributes[0].ParameterTypes[0].Type.FullType.Name);
        Assert.False(enumModel.Attributes[0].ParameterTypes[0].Type.FullType.IsNullable);
        foreach (var label in enumModel.Labels)
        {
            Assert.Empty(label.Attributes);
        }
    }
}
