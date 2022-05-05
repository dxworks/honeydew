using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Constructor.Info;

public class VisualBasicConstructorInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicConstructorInfoTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    new VisualBasicConstructorSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IConstructorType>>
                    {
                        new ConstructorInfoVisitor(),
                        new VisualBasicParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                        {
                            new ParameterInfoVisitor()
                        })
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ClassWithNoArgConstructor.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithNoArgConstructor(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Namespace1.Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Single(classType.Constructors);

        var constructor = classType.Constructors[0];
        Assert.Equal("New", constructor.Name);
        Assert.Empty(constructor.ParameterTypes);
        Assert.Equal("Public", constructor.AccessModifier);
        Assert.Equal("", constructor.Modifier);
    }

    [Theory]
    [FilePath("TestData/ClassWithPrivateConstructor.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithPrivateConstructor(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Namespace1.Module1.Class1", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Single(classType.Constructors);

        var constructor = classType.Constructors[0];
        Assert.Equal("Private", constructor.AccessModifier);
        Assert.Equal("", constructor.Modifier);
    }

    [Theory]
    [FilePath("TestData/ClassWithConstructorWithParameters.txt")]
    public async Task Extract_ShouldHaveInfo_WhenProvidedWithConstructorWithParameters(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Namespace1.Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Single(classType.Constructors);

        var constructor = classType.Constructors[0];
        Assert.Equal("New", constructor.Name);
        Assert.Equal("Public", constructor.AccessModifier);
        Assert.Equal("", constructor.Modifier);
        Assert.Equal(2, constructor.ParameterTypes.Count);

        var parameter1 = constructor.ParameterTypes[0];
        Assert.Equal("ByVal", parameter1.Modifier);
        Assert.Equal("String", parameter1.Type.Name);

        var parameter2 = constructor.ParameterTypes[1];
        Assert.Equal("ByVal", parameter2.Modifier);
        Assert.Equal("String", parameter2.Type.Name);
    }

    [Theory]
    [FilePath("TestData/ClassWithConstructorForCyclomaticComplexity.txt")]
    public async Task Extract_ShouldHaveCyclomaticComplexity_WhenProvidedWithConstructor(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Single(classType.Constructors);
        Assert.Equal("New", classType.Constructors[0].Name);
        Assert.Equal(4, classType.Constructors[0].CyclomaticComplexity);
    }
}
