using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.AccessField;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.AccessedFields;

public class PropertyAccessedFieldsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public PropertyAccessedFieldsTests()
    {
        var compositeVisitor = new CompositeVisitor();

        var accessedFieldsSetterVisitor = new AccessedFieldsSetterVisitor(new List<ICSharpAccessedFieldsVisitor>
        {
            new AccessFieldVisitor()
        });

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    accessedFieldsSetterVisitor
                })
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/AccessedFields/PropertyAccessedFields/PropertyAccessedNonStaticFieldAndPropertyFromOtherClass.txt")]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/AccessedFields/PropertyAccessedFields/PropertyAccessedStaticFieldAndPropertyFromOtherClass.txt")]
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
    [FileData(
        "TestData/CSharp/Metrics/Extraction/AccessedFields/PropertyAccessedFields/PropertyAccessedStaticFieldAndPropertyFromOtherClassArrowSyntax.txt")]
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
