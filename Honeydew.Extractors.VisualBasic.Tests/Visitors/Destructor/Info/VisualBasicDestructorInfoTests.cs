using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Destructor.Info;

public class VisualBasicDestructorInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicDestructorInfoTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    new VisualBasicDestructorSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDestructorType>>
                    {
                        new DestructorInfoVisitor()
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/BasicClassWithDestructor.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithDelegate(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Namespace1.Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.NotNull(classType.Destructor);
        Assert.Equal("Finalize", classType.Destructor.Name);
        Assert.Empty(classType.Destructor.ParameterTypes);
        Assert.Equal("Protected", classType.Destructor.AccessModifier);
        Assert.Equal("Overrides", classType.Destructor.Modifier);
    }

    [Theory]
    [FilePath("TestData/ClassWithDestructorWithCyclomaticComplexity.txt")]
    public async Task Extract_ShouldHaveCyclomaticComplexity_WhenProvidedWithDelegate(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.NotNull(classType.Destructor);
        Assert.Equal("Finalize", classType.Destructor.Name);
        Assert.Equal(4, classType.Destructor.CyclomaticComplexity);
    }
}
