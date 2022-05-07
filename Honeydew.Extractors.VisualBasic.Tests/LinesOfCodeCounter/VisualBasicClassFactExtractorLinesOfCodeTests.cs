using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.LinesOfCodeCounter;

public class VisualBasicClassFactExtractorLinesOfCodeTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicClassFactExtractorLinesOfCodeTests()
    {
        var linesOfCodeVisitor = new LinesOfCodeVisitor();
        var classTypeVisitors = new List<ITypeVisitor<IMembersClassType>>
        {
            linesOfCodeVisitor,
            new VisualBasicConstructorSetterVisitor(_loggerMock.Object,
                new List<ITypeVisitor<IConstructorType>>
                {
                    linesOfCodeVisitor
                }),
            new VisualBasicMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
            {
                linesOfCodeVisitor,
            }),
            new VisualBasicPropertySetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
            {
                linesOfCodeVisitor,
                new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IAccessorMethodType>>
                    {
                        linesOfCodeVisitor
                    })
            })
        };
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, classTypeVisitors),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, classTypeVisitors),
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, classTypeVisitors),
                new VisualBasicEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    linesOfCodeVisitor,
                }),
                new VisualBasicDelegateSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    linesOfCodeVisitor
                }),
                linesOfCodeVisitor
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ClassWithCommentsWithPropertyAndMethod.txt")]
    public async Task Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassWithMethodsAndProperties(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(15, compilationUnitType.Loc.SourceLines);
        Assert.Equal(9, compilationUnitType.Loc.EmptyLines);
        Assert.Equal(7, compilationUnitType.Loc.CommentedLines);

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(12, classModel.Loc.SourceLines);
        Assert.Equal(5, classModel.Loc.EmptyLines);
        Assert.Equal(4, classModel.Loc.CommentedLines);

        Assert.Equal(4, classModel.Methods[0].Loc.SourceLines);
        Assert.Equal(1, classModel.Methods[0].Loc.CommentedLines);
        Assert.Equal(1, classModel.Methods[0].Loc.EmptyLines);

        Assert.Equal(5, classModel.Properties[0].Loc.SourceLines);
        Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
        Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);
    }

    [Theory]
    [FilePath("TestData/ClassWithPropertyAndMethodAndDelegateWithComments.txt")]
    public async Task Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassAndDelegate(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(16, compilationUnitType.Loc.SourceLines);
        Assert.Equal(9, compilationUnitType.Loc.EmptyLines);
        Assert.Equal(8, compilationUnitType.Loc.CommentedLines);

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(12, classModel.Loc.SourceLines);
        Assert.Equal(6, classModel.Loc.EmptyLines);
        Assert.Equal(5, classModel.Loc.CommentedLines);

        Assert.Equal(4, classModel.Methods[0].Loc.SourceLines);
        Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
        Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

        Assert.Equal(5, classModel.Properties[0].Loc.SourceLines);
        Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
        Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);
    }

    [Theory]
    [FilePath("TestData/EnumWithComments.txt")]
    public async Task Extract_ShouldHaveLinesOfCode_WhenProvidedWithEnum(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(10, compilationUnitType.Loc.SourceLines);
        Assert.Equal(7, compilationUnitType.Loc.EmptyLines);
        Assert.Equal(2, compilationUnitType.Loc.CommentedLines);

        var classModel = (VisualBasicEnumModel)classTypes[0];
        Assert.Equal(7, classModel.Loc.SourceLines);
        Assert.Equal(4, classModel.Loc.EmptyLines);
        Assert.Equal(1, classModel.Loc.CommentedLines);
    }

    [Theory]
    [FilePath("TestData/DelegateOnOneLine.txt")]
    public async Task Extract_ShouldHaveLinesOfCode_WhenProvidedWithDelegateOnOneLine(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(3, compilationUnitType.Loc.SourceLines);
        Assert.Equal(1, compilationUnitType.Loc.EmptyLines);
        Assert.Equal(1, compilationUnitType.Loc.CommentedLines);

        var classModel = (VisualBasicDelegateModel)classTypes[0];
        Assert.Equal(1, classModel.Loc.SourceLines);
        Assert.Equal(0, classModel.Loc.EmptyLines);
        Assert.Equal(0, classModel.Loc.CommentedLines);
    }

    [Theory]
    [FilePath("TestData/ClassWithCommentsWithPropertyAndMethod.txt")]
    public async Task Extract_ShouldHaveLinesOfCode_WhenGivenPropertyWithGetAccessor(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var accessor = ((VisualBasicClassModel)classTypes[0]).Properties[0].Accessors[0];

        Assert.Equal(3, accessor.Loc.SourceLines);
        Assert.Equal(1, accessor.Loc.CommentedLines);
        Assert.Equal(1, accessor.Loc.EmptyLines);
    }
}
