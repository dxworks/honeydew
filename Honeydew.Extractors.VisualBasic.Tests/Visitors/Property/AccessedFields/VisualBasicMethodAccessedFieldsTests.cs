using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Property.AccessedFields;

public class VisualBasicMethodAccessedFieldsTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicMethodAccessedFieldsTests()
    {
        var accessedFieldsSetterVisitor = new VisualBasicAccessedFieldsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<AccessedField>>
            {
                new AccessFieldVisitor()
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
                        new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IAccessorMethodType>>
                            {
                                new MethodInfoVisitor(),
                                accessedFieldsSetterVisitor
                            })
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodAccessorWithAccessedFields.txt")]
    public async Task Extract_ShouldHaveAccessedFields_WhenGivenAccessorMethod(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("class", classType.ClassType);

        foreach (var accessor in classType.Properties[1].Accessors)
        {
            Assert.Equal(4, accessor.AccessedFields.Count);

            var accessedField1 = accessor.AccessedFields[0];
            Assert.Equal("Prop2", accessedField1.Name);
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField1.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField1.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField1.DefinitionClassName);

            var accessedField2 = accessor.AccessedFields[1];
            Assert.Equal("_F", accessedField2.Name);
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField2.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField2.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField2.DefinitionClassName);

            var accessedField3 = accessor.AccessedFields[2];
            Assert.Equal("Prop2", accessedField3.Name);
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField3.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField3.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField3.DefinitionClassName);

            var accessedField4 = accessor.AccessedFields[3];
            Assert.Equal("_F", accessedField4.Name);
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField4.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField4.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField4.DefinitionClassName);
        }
    }
}
