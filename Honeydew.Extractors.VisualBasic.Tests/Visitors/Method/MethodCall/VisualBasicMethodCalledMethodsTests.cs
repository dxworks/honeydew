using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Method.MethodCall;

public class VisualBasicMethodCalledMethodsTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicMethodCalledMethodsTests()
    {
        var calledMethodSetterVisitor = new VisualBasicCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor()
            });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    new VisualBasicMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                    {
                        new MethodInfoVisitor(),
                        calledMethodSetterVisitor
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodWithCalledMethods.txt")]
    public async Task Extract_ShouldHaveCalledMethods_WhenGivenMethod(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("class", classType.ClassType);

        for (var i = 0; i < 2; i++)
        {
            var calledMethod1 = classType.Methods[i].CalledMethods[0];
            Assert.Equal("Function1", calledMethod1.Name);
            Assert.Empty(calledMethod1.ParameterTypes);
            Assert.Equal("Namespace1.Module1.User", calledMethod1.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", calledMethod1.DefinitionClassName);

            var calledMethod2 = classType.Methods[i].CalledMethods[1];
            Assert.Equal("Method1", calledMethod2.Name);
            Assert.Single(calledMethod2.ParameterTypes);
            Assert.Equal("Double", calledMethod2.ParameterTypes[0].Type.Name);
            Assert.Equal("Namespace1.Module1.User", calledMethod2.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", calledMethod2.DefinitionClassName);
        }
    }
}
