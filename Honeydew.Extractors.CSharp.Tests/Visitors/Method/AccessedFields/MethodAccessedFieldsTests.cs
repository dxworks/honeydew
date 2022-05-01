using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.AccessedFields;

public class MethodAccessedFieldsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public MethodAccessedFieldsTests()
    {
        var accessedFieldsSetterVisitor = new CSharpAccessedFieldsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<AccessedField>>
            {
                new AccessFieldVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            accessedFieldsSetterVisitor
                        }),
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }


    [Theory]
    [FileData("TestData/MethodAccessedNonStaticFieldAndPropertyFromClass.txt")]
    [FileData("TestData/MethodAccessedStaticFieldAndPropertyFromClass.txt")]
    public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesFieldsAndPropertiesFromInsideTheClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(2, methodType.AccessedFields.Count);

            Assert.Equal("Field1", methodType.AccessedFields[0].Name);
            Assert.Equal("Property1", methodType.AccessedFields[1].Name);

            foreach (var accessedField in methodType.AccessedFields)
            {
                Assert.Equal(classModel.Name, accessedField.DefinitionClassName);
                Assert.Equal(classModel.Name, accessedField.LocationClassName);
            }
        }

        foreach (var accessedField in classModel.Methods[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Methods[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }
    }

    [Theory]
    [FileData("TestData/MethodAccessedNonStaticFieldAndPropertyFromOtherClass.txt")]
    [FileData("TestData/MethodAccessedStaticFieldAndPropertyFromOtherClass.txt")]
    public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesFieldsAndPropertiesFromOtherClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(2, methodType.AccessedFields.Count);

            Assert.Equal("Field1", methodType.AccessedFields[0].Name);
            Assert.Equal("Property1", methodType.AccessedFields[1].Name);

            foreach (var accessedField in methodType.AccessedFields)
            {
                Assert.Equal(classTypes[1].Name, accessedField.DefinitionClassName);
                Assert.Equal(classTypes[1].Name, accessedField.LocationClassName);
            }
        }

        foreach (var accessedField in classModel.Methods[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Methods[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }
    }

    [Theory]
    [FileData("TestData/MethodAccessedNonStaticFieldAndPropertyFromExternClass.txt")]
    [FileData("TestData/MethodAccessedStaticFieldAndPropertyFromExternClass.txt")]
    public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesFieldsAndPropertiesFromExternClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(2, methodType.AccessedFields.Count);

            Assert.Equal("Field1", methodType.AccessedFields[0].Name);
            Assert.Equal("Property1", methodType.AccessedFields[1].Name);

            foreach (var accessedField in methodType.AccessedFields)
            {
                Assert.Equal("ExternClass", accessedField.DefinitionClassName);
                Assert.Equal("ExternClass", accessedField.LocationClassName);
            }
        }

        foreach (var accessedField in classModel.Methods[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Methods[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }
    }

    [Theory]
    [FileData("TestData/MethodAccessedFieldWithBracketOperator.txt")]
    public void Extract_ShouldNotHaveAccessedFields_WhenGivenArrayAccessor(string fileContent) // field[]
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(2, methodType.AccessedFields.Count);

            Assert.Equal("Field1", methodType.AccessedFields[0].Name);
            Assert.Equal("Property1", methodType.AccessedFields[1].Name);

            foreach (var accessedField in methodType.AccessedFields)
            {
                Assert.Equal("Namespace1.Class1", accessedField.DefinitionClassName);
                Assert.Equal("Namespace1.Class1", accessedField.LocationClassName);
            }
        }

        foreach (var accessedField in classModel.Methods[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        foreach (var accessedField in classModel.Methods[1].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
        }
    }

    [Theory]
    [FileData("TestData/MethodAccessedFieldWithFromBaseClass.txt")]
    public void Extract_ShouldHaveAccessedFields_WhenGivenFieldsFromBaseClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[2];
        var methodType = classModel.Methods[0];

        Assert.Equal(4, methodType.AccessedFields.Count);

        Assert.Equal("Field", methodType.AccessedFields[0].Name);
        Assert.Equal("Prop", methodType.AccessedFields[1].Name);
        Assert.Equal("P", methodType.AccessedFields[2].Name);
        Assert.Equal("S", methodType.AccessedFields[3].Name);

        Assert.Equal("Namespace1.Base", methodType.AccessedFields[0].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[0].LocationClassName);

        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[1].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[1].LocationClassName);

        Assert.Equal("Namespace1.Base", methodType.AccessedFields[2].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[2].LocationClassName);

        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[3].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[3].LocationClassName);
    }
}
