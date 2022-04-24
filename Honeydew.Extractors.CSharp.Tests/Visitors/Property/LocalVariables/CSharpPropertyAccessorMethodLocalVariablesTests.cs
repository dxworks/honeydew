using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Property.LocalVariables;

public class CSharpPropertyAccessorMethodLocalVariablesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpPropertyAccessorMethodLocalVariablesTests()
    {
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
                                    new CSharpLocalVariablesTypeSetterVisitor(_loggerMock.Object,
                                        new List<ITypeVisitor<ILocalVariableType>>
                                        {
                                            new LocalVariableInfoVisitor()
                                        })
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/MethodAccessorWithPrimitiveLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Properties.Count);

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(4, accessor.LocalVariableTypes.Count);
                Assert.Equal("int", accessor.LocalVariableTypes[0].Type.Name);
                Assert.Equal("int", accessor.LocalVariableTypes[1].Type.Name);
                Assert.Equal("int", accessor.LocalVariableTypes[2].Type.Name);
                Assert.Equal("string", accessor.LocalVariableTypes[3].Type.Name);
            }
        }
    }

    [Theory]
    [FileData("TestData/MethodAccessorWithCustomClassLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(5, accessor.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Parent", accessor.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2", accessor.LocalVariableTypes[3].Type.Name);
                Assert.Equal("Namespace1.Class3", accessor.LocalVariableTypes[4].Type.Name);
            }
        }


        Assert.Equal("Namespace1.Parent", classModel.Properties[0].Accessors[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Parent", classModel.Properties[0].Accessors[0].LocalVariableTypes[2].Type.Name);
        Assert.Equal("Namespace1.Class2", classModel.Properties[0].Accessors[1].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Class3", classModel.Properties[0].Accessors[1].LocalVariableTypes[2].Type.Name);

        Assert.Equal("Namespace1.Parent", classModel.Properties[1].Accessors[1].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Parent", classModel.Properties[1].Accessors[1].LocalVariableTypes[2].Type.Name);
        Assert.Equal("Namespace1.Class2", classModel.Properties[1].Accessors[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Class3", classModel.Properties[1].Accessors[0].LocalVariableTypes[2].Type.Name);

        Assert.Equal("Namespace1.Parent", classModel.Properties[2].Accessors[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Parent", classModel.Properties[2].Accessors[0].LocalVariableTypes[2].Type.Name);
        Assert.Equal("Namespace1.Class2", classModel.Properties[2].Accessors[1].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Class3", classModel.Properties[2].Accessors[1].LocalVariableTypes[2].Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodAccessorWithExternClassLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;
        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(3, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(3, accessor.LocalVariableTypes.Count);
                foreach (var localVariableType in accessor.LocalVariableTypes)
                {
                    Assert.Equal("ExternClass", localVariableType.Type.Name);
                }
            }
        }
    }

    [Theory]
    [FileData("TestData/MethodAccessorWithArrayLocalVariable.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(3, accessor.LocalVariableTypes.Count);
                Assert.Equal("int[]", accessor.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2[]", accessor.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass[]", accessor.LocalVariableTypes[2].Type.Name);
            }
        }
    }

    [Theory]
    [FileData("TestData/PropertyWithRefLocals.txt")]
    public void Extract_ShouldHaveRefModifier_WhenGivenPropertyWithLocalVariables(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                foreach (var localVariableType in accessor.LocalVariableTypes)
                {
                    Assert.Equal("ref", localVariableType.Modifier);
                }
            }
        }
    }

    [Theory]
    [FileData("TestData/PropertyWithRefReadonlyLocals.txt")]
    public void Extract_ShouldHaveRefReadonlyModifier_WhenGivenPropertyWithLocalVariables(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                foreach (var localVariableType in accessor.LocalVariableTypes)
                {
                    Assert.Equal("ref readonly", localVariableType.Modifier);
                }
            }
        }
    }
}
