using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Field.Info;

public class VisualBasicFieldInfoTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicFieldInfoTests()
    {
        var fileSetterVisitor = new VisualBasicFieldSetterClassVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IFieldType>>
            {
                new FieldInfoVisitor()
            });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    fileSetterVisitor
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    fileSetterVisitor
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ClassWithField.txt")]
    [FilePath("TestData/StructureWithField.txt")]
    public async Task Extract_ShouldPropertyHaveInfo_WhenProvidedWithProperties(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("Customer", classType.Name);
        Assert.Equal("", classType.ContainingClassName);
        Assert.Equal("Global", classType.ContainingNamespaceName);
        Assert.Equal("", classType.ContainingModuleName);

        Assert.Equal(4, classType.Fields.Count);
        var property1 = classType.Fields[0];
        Assert.Equal("givenName", property1.Name);
        Assert.Equal("Public", property1.AccessModifier);
        Assert.Equal("ReadOnly", property1.Modifier);
        Assert.Equal("String", property1.Type.Name);
        Assert.Equal("String", property1.Type.FullType.Name);

        var property2 = classType.Fields[1];
        Assert.Equal("familyName", property2.Name);
        Assert.Equal("Friend", property2.AccessModifier);
        Assert.Equal("", property2.Modifier);
        Assert.Equal("String", property2.Type.Name);
        Assert.Equal("String", property2.Type.FullType.Name);

        var property3 = classType.Fields[2];
        Assert.Equal("salary", property3.Name);
        Assert.Equal("Private", property3.AccessModifier);
        Assert.Equal("", property3.Modifier);
        Assert.Equal("Decimal", property3.Type.Name);
        Assert.Equal("Decimal", property3.Type.FullType.Name);
        
        var property4 = classType.Fields[3];
        Assert.Equal("salary2", property4.Name);
        Assert.Equal("Private", property4.AccessModifier);
        Assert.Equal("", property4.Modifier);
        Assert.Equal("Decimal", property4.Type.Name);
        Assert.Equal("Decimal", property4.Type.FullType.Name);
    }
}
