using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Enum.Labels;

public class VisualBasicEnumLabelInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicEnumLabelInfoTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    new BaseInfoEnumVisitor(),
                    new VisualBasicEnumLabelsSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumLabelType>>
                    {
                        new BasicEnumLabelInfoVisitor()
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/BasicEnum.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithEnum(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var enumModel = (VisualBasicEnumModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Namespace1.Module1.Enum1", enumModel.Name);
        Assert.Equal("enum", enumModel.ClassType);
        Assert.Equal(3, enumModel.Labels.Count);
        Assert.Equal("Value1", enumModel.Labels[0].Name);
        Assert.Equal("Value2", enumModel.Labels[1].Name);
        Assert.Equal("Value3", enumModel.Labels[2].Name);
    }
}
