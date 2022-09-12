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
    public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatNonStaticAccessesFieldsAndPropertiesFromOtherClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(4, methodType.AccessedFields.Count);

            Assert.Equal("_class2", methodType.AccessedFields[0].Name);
            Assert.Equal("Namespace1.Class1", methodType.AccessedFields[0].DefinitionClassName);
            Assert.Equal("Namespace1.Class1", methodType.AccessedFields[0].LocationClassName);
            
            Assert.Equal("Field1", methodType.AccessedFields[1].Name);
            Assert.Equal("Namespace1.Class2", methodType.AccessedFields[1].DefinitionClassName);
            Assert.Equal("Namespace1.Class2", methodType.AccessedFields[1].LocationClassName);
            
            Assert.Equal("_class2", methodType.AccessedFields[2].Name);
            Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].DefinitionClassName);
            Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].LocationClassName);
            
            Assert.Equal("Property1", methodType.AccessedFields[3].Name);
            Assert.Equal("Namespace1.Class2", methodType.AccessedFields[3].DefinitionClassName);
            Assert.Equal("Namespace1.Class2", methodType.AccessedFields[3].LocationClassName);
        }

        foreach (var accessedField in classModel.Methods[0].AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }

        Assert.Equal(AccessedField.AccessKind.Getter, classModel.Methods[1].AccessedFields[0].Kind);
        Assert.Equal(AccessedField.AccessKind.Setter, classModel.Methods[1].AccessedFields[1].Kind);
        Assert.Equal(AccessedField.AccessKind.Getter, classModel.Methods[1].AccessedFields[2].Kind);
        Assert.Equal(AccessedField.AccessKind.Setter, classModel.Methods[1].AccessedFields[3].Kind);
    }

    [Theory]
    [FileData("TestData/MethodAccessedStaticFieldAndPropertyFromOtherClass.txt")]
    public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesStaticFieldsAndPropertiesFromOtherClass(
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

        Assert.Equal(8, methodType.AccessedFields.Count);

        foreach (var accessedField in methodType.AccessedFields)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
        }
        
        Assert.Equal("d", methodType.AccessedFields[0].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[0].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[0].LocationClassName);

        Assert.Equal("Field", methodType.AccessedFields[1].Name);
        Assert.Equal("Namespace1.Base", methodType.AccessedFields[1].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[1].LocationClassName);

        Assert.Equal("d", methodType.AccessedFields[2].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].LocationClassName);

        Assert.Equal("Prop", methodType.AccessedFields[3].Name);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[3].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[3].LocationClassName);

        Assert.Equal("d", methodType.AccessedFields[4].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[4].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[4].LocationClassName);

        Assert.Equal("P", methodType.AccessedFields[5].Name);
        Assert.Equal("Namespace1.Base", methodType.AccessedFields[5].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[5].LocationClassName);
        
        Assert.Equal("d", methodType.AccessedFields[6].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[6].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[6].LocationClassName);
        
        Assert.Equal("S", methodType.AccessedFields[7].Name);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[7].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", methodType.AccessedFields[7].LocationClassName);
    }

    [Theory]
    [FileData("TestData/MethodAccessedFieldThatAccessesMember.txt")]
    public void Extract_ShouldHaveAccessedField_WhenGivenFiledThatCallsMethod(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[1];
        var methodType = classModel.Methods[0];

        Assert.Equal(5, methodType.AccessedFields.Count);

        Assert.Equal("c1", methodType.AccessedFields[0].Name);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[0].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[0].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[0].Kind);

        Assert.Equal("c2", methodType.AccessedFields[1].Name);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[1].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[1].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[1].Kind);

        Assert.Equal("Field", methodType.AccessedFields[2].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Setter, methodType.AccessedFields[2].Kind);

        Assert.Equal("c1", methodType.AccessedFields[3].Name);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[3].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[3].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[3].Kind);

        Assert.Equal("X", methodType.AccessedFields[4].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[4].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[4].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[4].Kind);
    }
}
