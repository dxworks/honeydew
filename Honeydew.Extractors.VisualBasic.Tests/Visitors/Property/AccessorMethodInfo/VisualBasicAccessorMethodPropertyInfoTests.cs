using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Property.AccessorMethodInfo;

public class VisualBasicAccessorMethodPropertyInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicAccessorMethodPropertyInfoTests()
    {
        var propertySetterVisitor = new VisualBasicPropertySetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IPropertyType>>
            {
                new PropertyInfoVisitor(),
                new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IAccessorMethodType>>
                    {
                        new MethodInfoVisitor(),
                        new VisualBasicParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                        {
                            new ParameterInfoVisitor(),
                        }),
                        new VisualBasicReturnValueSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IReturnValueType>>
                            {
                                new ReturnValueInfoVisitor()
                            })
                    })
            });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    propertySetterVisitor
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    propertySetterVisitor
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ClassWithProperty.txt")]
    [FilePath("TestData/StructureWithProperty.txt")]
    public async Task Extract_ShouldPropertyHaveInfo_WhenProvidedWithProperties(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Customer", classType.Name);
        Assert.Equal("", classType.ContainingClassName);
        Assert.Equal("Global", classType.ContainingNamespaceName);
        Assert.Equal("", classType.ContainingModuleName);

        Assert.Single(classType.Properties);
        var property = classType.Properties[0];
        Assert.Equal("Prop2", property.Name);
        Assert.Equal("Friend", property.AccessModifier);
        Assert.Equal("String", property.Type.Name);
        Assert.Equal(2, property.Accessors.Count);

        var getAccessor = property.Accessors[0];
        Assert.Equal("Get", getAccessor.Name);
        Assert.Equal("Public", getAccessor.AccessModifier);
        Assert.Empty(getAccessor.ParameterTypes);
        Assert.Equal("String", getAccessor.ReturnValue.Type.Name);
        
        var setAccessor = property.Accessors[1];
        Assert.Equal("Set", setAccessor.Name);
        Assert.Equal("Public", setAccessor.AccessModifier);
        Assert.Single(setAccessor.ParameterTypes);
        Assert.Equal("ByVal",setAccessor.ParameterTypes[0].Modifier);
        Assert.Equal("String",setAccessor.ParameterTypes[0].Type.Name);
        Assert.Equal("Void", setAccessor.ReturnValue.Type.Name);
    }

    [Theory]
    [FilePath("TestData/ClassWithPropertyForCyclomaticComplexity.txt")]
    public async Task Extract_ShouldHaveCyclomaticComplexity_WhenProvidedWithPropertyWithAccessorMethods(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        var property = classType.Properties[0];
        Assert.Equal(2, property.Accessors.Count);
        Assert.Equal("Get", property.Accessors[0].Name);
        Assert.Equal(4, property.Accessors[0].CyclomaticComplexity);
        Assert.Equal("Set", property.Accessors[1].Name);
        Assert.Equal(4, property.Accessors[1].CyclomaticComplexity);   
    }
}
