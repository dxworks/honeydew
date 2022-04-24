using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Constructor.AccessedFields;

public class ConstructorAccessedFieldsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public ConstructorAccessedFieldsTests()
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
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                accessedFieldsSetterVisitor
                            })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ConstructorAccessedNonStaticFieldAndPropertyFromClass.txt")]
    public void
        Extract_ShouldHaveAccessedFields_WhenGivenConstructorThatAccessesFieldsAndPropertiesFromInsideTheClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(2, constructorType.AccessedFields.Count);

            Assert.Equal("Field1", constructorType.AccessedFields[0].Name);
            Assert.Equal("Property1", constructorType.AccessedFields[1].Name);

            foreach (var accessedField in constructorType.AccessedFields)
            {
                Assert.Equal(classModel.Name, accessedField.DefinitionClassName);
                Assert.Equal(classModel.Name, accessedField.LocationClassName);
            }
        }

        foreach (var accessedField in classModel.Constructors[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Constructors[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }
    }

    [Theory]
    [FileData("TestData/ConstructorAccessedStaticFieldAndPropertyFromClass.txt")]
    public void
        Extract_ShouldHaveAccessedFields_WhenGivenStaticConstructorThatAccessesFieldsAndPropertiesFromInsideTheClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        foreach (var constructorType in classModel.Constructors)
        {
            Assert.Equal(4, constructorType.AccessedFields.Count);

            Assert.Equal("Field1", constructorType.AccessedFields[0].Name);
            Assert.Equal("Property1", constructorType.AccessedFields[1].Name);
            Assert.Equal("Field1", constructorType.AccessedFields[2].Name);
            Assert.Equal("Property1", constructorType.AccessedFields[3].Name);

            foreach (var accessedField in constructorType.AccessedFields)
            {
                Assert.Equal(classModel.Name, accessedField.DefinitionClassName);
                Assert.Equal(classModel.Name, accessedField.LocationClassName);
            }
        }

        Assert.Equal(AccessedField.AccessKind.Getter, classModel.Constructors[0].AccessedFields[0].Kind);
        Assert.Equal(AccessedField.AccessKind.Getter, classModel.Constructors[0].AccessedFields[1].Kind);
        Assert.Equal(AccessedField.AccessKind.Setter, classModel.Constructors[0].AccessedFields[2].Kind);
        Assert.Equal(AccessedField.AccessKind.Setter, classModel.Constructors[0].AccessedFields[3].Kind);
    }
}
