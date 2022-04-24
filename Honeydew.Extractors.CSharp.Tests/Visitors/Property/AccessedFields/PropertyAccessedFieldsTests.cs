using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Property.AccessedFields;

public class PropertyAccessedFieldsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public PropertyAccessedFieldsTests()
    {
        var accessedFieldsSetterVisitor = new CSharpAccessedFieldsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<AccessedField>>
            {
                new AccessFieldVisitor()
            });
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterPropertyVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    new MethodInfoVisitor(),
                                    accessedFieldsSetterVisitor
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/PropertyAccessedNonStaticFieldAndPropertyFromOtherClass.txt")]
    [FileData("TestData/PropertyAccessedStaticFieldAndPropertyFromOtherClass.txt")]
    public void
        Extract_ShouldHaveAccessedFields_WhenGivenPropertyAccessorThatAccessesFieldsAndPropertiesFromOtherClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(2, accessor.AccessedFields.Count);

                Assert.Equal("Field1", accessor.AccessedFields[0].Name);
                Assert.Equal("Property1", accessor.AccessedFields[1].Name);

                foreach (var accessedField in accessor.AccessedFields)
                {
                    Assert.Equal(classTypes[1].Name, accessedField.DefinitionClassName);
                    Assert.Equal(classTypes[1].Name, accessedField.LocationClassName);
                }
            }
        }

        foreach (var accessedField in classModel.Properties[0].Accessors[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Properties[0].Accessors[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Properties[1].Accessors[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Properties[1].Accessors[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }
    }

    [Theory]
    [FileData("TestData/PropertyAccessedStaticFieldAndPropertyFromOtherClassArrowSyntax.txt")]
    public void
        Extract_ShouldHaveAccessedFields_WhenGivenPropertyArrowSyntaxThatAccessesFieldsAndPropertiesFromOtherClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(2, accessor.AccessedFields.Count);

                Assert.Equal("Field1", accessor.AccessedFields[0].Name);
                Assert.Equal("Property1", accessor.AccessedFields[1].Name);

                foreach (var accessedField in accessor.AccessedFields)
                {
                    Assert.Equal(classTypes[1].Name, accessedField.DefinitionClassName);
                    Assert.Equal(classTypes[1].Name, accessedField.LocationClassName);
                }
            }
        }

        foreach (var accessedField in classModel.Properties[0].Accessors[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }
    }
}
