using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Property.MethodCall;

public class VisualBasicPropertyCalledMethodsTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicPropertyCalledMethodsTests()
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
                    new VisualBasicPropertySetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                    {
                       new PropertyInfoVisitor(),
                       new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAccessorMethodType>>
                       {
                           new MethodInfoVisitor(),
                           calledMethodSetterVisitor
                       })
                    }),
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodAccessorWithCalledMethods.txt")]
    public async Task Extract_ShouldHaveCalledMethods_WhenGivenMethod(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("class", classType.ClassType);

        var getAccessor = classType.Properties[0].Accessors[0];
        Assert.Equal("Get", getAccessor.Name);
        
        var getCalledMethod1 = getAccessor.CalledMethods[0];
        Assert.Equal("Function1", getCalledMethod1.Name);
        Assert.Empty(getCalledMethod1.ParameterTypes);
        Assert.Equal("Namespace1.Module1.User", getCalledMethod1.LocationClassName);
        Assert.Equal("Namespace1.Module1.User", getCalledMethod1.DefinitionClassName);

        var getCalledMethod2 =  getAccessor.CalledMethods[1];
        Assert.Equal("Method1", getCalledMethod2.Name);
        Assert.Single(getCalledMethod2.ParameterTypes);
        Assert.Equal("Double", getCalledMethod2.ParameterTypes[0].Type.Name);
        Assert.Equal("Namespace1.Module1.User", getCalledMethod2.LocationClassName);
        Assert.Equal("Namespace1.Module1.User", getCalledMethod2.DefinitionClassName);

        var setAccessor = classType.Properties[0].Accessors[1];
        Assert.Equal("Set", setAccessor.Name);

        var setCalledMethod1 = setAccessor.CalledMethods[0];
        Assert.Equal("Function1", setCalledMethod1.Name);
        Assert.Empty(setCalledMethod1.ParameterTypes);
        Assert.Equal("Namespace1.Module1.User", setCalledMethod1.LocationClassName);
        Assert.Equal("Namespace1.Module1.User", setCalledMethod1.DefinitionClassName);
        
        var setCalledMethod2 =  setAccessor.CalledMethods[1];
        Assert.Equal("Method1", setCalledMethod2.Name);
        Assert.Single(setCalledMethod2.ParameterTypes);
        Assert.Equal("Double", setCalledMethod2.ParameterTypes[0].Type.Name);
        Assert.Equal("Namespace1.Module1.User", setCalledMethod2.LocationClassName);
        Assert.Equal("Namespace1.Module1.User", setCalledMethod2.DefinitionClassName);
    }
}
