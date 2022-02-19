using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.AccessField;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.AccessedFields;

public class ConstructorAccessedFieldsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public ConstructorAccessedFieldsTests()
    {
        var compositeVisitor = new CompositeVisitor();

        var accessedFieldsSetterVisitor = new AccessedFieldsSetterVisitor(new List<ICSharpAccessedFieldsVisitor>
        {
            new AccessFieldVisitor()
        });

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                accessedFieldsSetterVisitor
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }


    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/AccessedFields/ConstructorAccessedFields/ConstructorAccessedNonStaticFieldAndPropertyFromClass.txt")]
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
    [FileData(
        "TestData/CSharp/Metrics/Extraction/AccessedFields/ConstructorAccessedFields/ConstructorAccessedStaticFieldAndPropertyFromClass.txt")]
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
