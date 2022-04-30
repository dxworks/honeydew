using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Constructor.ConstructorCalls;

public class VisualBasicConstructorCallsTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicConstructorCallsTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    new VisualBasicConstructorClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IConstructorType>>
                    {
                        new ConstructorInfoVisitor(),
                        new ConstructorCallsVisitor(),
                        new VisualBasicParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                        {
                            new ParameterInfoVisitor()
                        }),
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ConstructorCallChain.txt")]
    public async Task Extract_ShouldHaveConstructorCalls_WhenProvidedWithConstructorCallChain(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Module1.User", classType.Name);
        Assert.Equal("class", classType.ClassType);
        Assert.Equal(3, classType.Constructors.Count);

        var constructor1 = classType.Constructors[0];
        Assert.Empty(constructor1.ParameterTypes);
        Assert.Empty(constructor1.CalledMethods);

        var constructor2 = classType.Constructors[1];
        Assert.Single(constructor2.ParameterTypes);
        Assert.Equal("String", constructor2.ParameterTypes[0].Type.Name);
        Assert.Single(constructor2.CalledMethods);
        Assert.Equal("New", constructor2.CalledMethods[0].Name);
        Assert.Equal("Module1.User", constructor2.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Module1.User", constructor2.CalledMethods[0].LocationClassName);
        Assert.Empty(constructor2.CalledMethods[0].ParameterTypes);

        var constructor3 = classType.Constructors[2];
        Assert.Equal(2, constructor3.ParameterTypes.Count);
        Assert.Equal("String", constructor3.ParameterTypes[0].Type.Name);
        Assert.Equal("String", constructor3.ParameterTypes[1].Type.Name);
        Assert.Single(constructor3.CalledMethods);
        Assert.Equal("New", constructor3.CalledMethods[0].Name);
        Assert.Equal("Module1.User", constructor3.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Module1.User", constructor3.CalledMethods[0].LocationClassName);
        Assert.Single(constructor3.CalledMethods[0].ParameterTypes);
        Assert.Equal("String", constructor3.CalledMethods[0].ParameterTypes[0].Type.Name);
    }

    [Theory]
    [FilePath("TestData/BaseConstructorCall.txt")]
    public async Task Extract_ShouldHaveConstructorCalls_WhenProvidedWithBaseConstructorCall(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        Assert.Equal(2, compilationUnitType.ClassTypes.Count);

        var windowClass = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Equal("Window", windowClass.Name);
        Assert.Equal("class", windowClass.ClassType);
        Assert.Single(windowClass.Constructors);

        var windowClassConstructor = windowClass.Constructors[0];
        Assert.Equal(2, windowClassConstructor.ParameterTypes.Count);
        Assert.Equal("Integer", windowClassConstructor.ParameterTypes[0].Type.Name);
        Assert.Equal("Integer", windowClassConstructor.ParameterTypes[1].Type.Name);
        Assert.Empty(windowClassConstructor.CalledMethods);

        var listBoxClass = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
        Assert.Equal("ListBox", listBoxClass.Name);
        Assert.Equal("class", listBoxClass.ClassType);
        Assert.Single(listBoxClass.Constructors);

        var listBoxClassConstructor = listBoxClass.Constructors[0];
        Assert.Equal(3, listBoxClassConstructor.ParameterTypes.Count);
        Assert.Equal("Integer", listBoxClassConstructor.ParameterTypes[0].Type.Name);
        Assert.Equal("Integer", listBoxClassConstructor.ParameterTypes[1].Type.Name);
        Assert.Equal("String", listBoxClassConstructor.ParameterTypes[2].Type.Name);
        Assert.Single(listBoxClassConstructor.CalledMethods);
        Assert.Equal("New", listBoxClassConstructor.CalledMethods[0].Name);
        Assert.Equal("Window", listBoxClassConstructor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Window", listBoxClassConstructor.CalledMethods[0].LocationClassName);
        Assert.Equal("Integer", listBoxClassConstructor.CalledMethods[0].ParameterTypes[0].Type.Name);
        Assert.Equal("Integer", listBoxClassConstructor.CalledMethods[0].ParameterTypes[1].Type.Name);
    }
}
