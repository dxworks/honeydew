using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Others.GotoStatement;

public class VisualBasicGotoStatementVisitorTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicGotoStatementVisitorTests()
    {
        var gotoStatementVisitor = new GotoStatementVisitor();

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
                                gotoStatementVisitor,
                            }),
                        new VisualBasicMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            gotoStatementVisitor,
                        }),
                        new VisualBasicDestructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IDestructorType>>
                            {
                                new DestructorInfoVisitor(),
                                gotoStatementVisitor,
                            }),
                        new VisualBasicPropertySetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    new MethodInfoVisitor(),
                                    gotoStatementVisitor,
                                })
                        })
                    })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/GotoInConstructorTests.txt")]
    public async Task Extract_ShouldExtractGotoStatements_WhenProvidedConstructor(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];

        TestGotoStatementInMetrics(classModel.Constructors[0].Metrics);
    }

    [Theory]
    [FilePath("TestData/GotoInDestructorTests.txt")]
    public async Task Extract_ShouldExtractGotoStatements_WhenProvidedDestructor(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];

        TestGotoStatementInMetrics(classModel.Destructor.Metrics);
    }

    [Theory]
    [FilePath("TestData/GotoInMethodTests.txt")]
    public async Task Extract_ShouldExtractGotoStatements_WhenProvidedMethod(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];

        TestGotoStatementInMetrics(classModel.Methods[0].Metrics);
        TestGotoStatementInMetrics(classModel.Methods[1].Metrics);
    }

    [Theory]
    [FilePath("TestData/GotoInPropertyAccessorTests.txt")]
    public async Task Extract_ShouldExtractGotoStatements_WhenProvidedPropertyAccessors(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];

        TestGotoStatementInMetrics(classModel.Properties[0].Accessors[0].Metrics);
        TestGotoStatementInMetrics(classModel.Properties[0].Accessors[1].Metrics);
    }

    private static void TestGotoStatementInMetrics(IList<MetricModel> metrics)
    {
        Assert.Single(metrics);
        Assert.Equal(typeof(GotoStatementVisitor).FullName, metrics[0].ExtractorName);
        Assert.Equal(typeof(int).FullName, metrics[0].ValueType);
        Assert.Equal(3, (int)metrics[0].Value!);
    }
}
