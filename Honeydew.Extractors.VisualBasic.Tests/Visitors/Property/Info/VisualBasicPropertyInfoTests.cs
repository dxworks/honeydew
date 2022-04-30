using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Property.Info;

public class VisualBasicPropertyInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicPropertyInfoTests()
    {
        var propertySetterVisitor = new VisualBasicPropertySetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IPropertyType>>
            {
                new PropertyInfoVisitor()
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
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    propertySetterVisitor
                })
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

        Assert.Equal(3, classType.Properties.Count);
        var property1 = classType.Properties[0];
        Assert.Equal("Tags", property1.Name);
        Assert.Equal("Public", property1.AccessModifier);
        Assert.Equal("ReadOnly", property1.Modifier);
        Assert.Equal("List(Of String)", property1.Type.Name);
        Assert.Equal("List", property1.Type.FullType.Name);
        Assert.Equal("String", property1.Type.FullType.ContainedTypes[0].Name);
        Assert.Equal(1, property1.CyclomaticComplexity);

        var property2 = classType.Properties[1];
        Assert.Equal("Name", property2.Name);
        Assert.Equal("Friend", property2.AccessModifier);
        Assert.Equal("", property2.Modifier);
        Assert.Equal("String", property2.Type.Name);
        Assert.Equal("String", property2.Type.FullType.Name);
        Assert.Equal(1, property2.CyclomaticComplexity);

        var property3 = classType.Properties[2];
        Assert.Equal("Prop2", property3.Name);
        Assert.Equal("Friend", property3.AccessModifier);
        Assert.Equal("", property3.Modifier);
        Assert.Equal("String", property3.Type.Name);
        Assert.Equal("String", property3.Type.FullType.Name);
        Assert.Equal(1, property3.CyclomaticComplexity);
    }
    
    [Theory]
    [FilePath("TestData/InterfaceWithProperty.txt")]
    public async Task Extract_ShouldPropertyHaveInfo_WhenProvidedInterfaceWithProperties(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Customer", classType.Name);
        Assert.Equal("", classType.ContainingClassName);
        Assert.Equal("Global", classType.ContainingNamespaceName);
        Assert.Equal("", classType.ContainingModuleName);

        Assert.Equal(2, classType.Properties.Count);
        var property1 = classType.Properties[0];
        Assert.Equal("Tags", property1.Name);
        Assert.Equal("Public", property1.AccessModifier);
        Assert.Equal("ReadOnly", property1.Modifier);
        Assert.Equal("List(Of String)", property1.Type.Name);
        Assert.Equal("List", property1.Type.FullType.Name);
        Assert.Equal("String", property1.Type.FullType.ContainedTypes[0].Name);
        Assert.Equal(1, property1.CyclomaticComplexity);

        var property2 = classType.Properties[1];
        Assert.Equal("Name", property2.Name);
        Assert.Equal("Friend", property2.AccessModifier);
        Assert.Equal("", property2.Modifier);
        Assert.Equal("String", property2.Type.Name);
        Assert.Equal("String", property2.Type.FullType.Name);
        Assert.Equal(1, property2.CyclomaticComplexity);
    }
}
