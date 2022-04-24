using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Delegate.GenericType;

public class CSharpGenericDelegateTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpGenericDelegateTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpDelegateSetterCompilationUnitVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    new BaseInfoDelegateVisitor(),
                    new CSharpGenericParameterSetterVisitor(_loggerMock.Object,
                        new List<ITypeVisitor<IGenericParameterType>>
                        {
                            new GenericParameterInfoVisitor()
                        })
                })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/DelegateWithOneGenericParam.txt")]
    public void Extract_ShouldHaveDelegateNameOfGenericType_WhenProvidedWithGenericDelegate(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (DelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1<T>", classModel.Name);
        Assert.Equal(1, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("", classModel.GenericParameters[0].Modifier);
        Assert.Empty(classModel.GenericParameters[0].Constraints);
    }

    [Theory]
    [FileData("TestData/DelegateWithMultipleGenericParams.txt")]
    public void Extract_ShouldHaveDelegateNameGenericTypeWithMultipleContainedTypes_WhenProvidedWithGenericDelegate(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (DelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1<T,R,K>", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("", classModel.GenericParameters[0].Modifier);
        Assert.Equal("R", classModel.GenericParameters[1].Name);
        Assert.Equal("", classModel.GenericParameters[1].Modifier);
        Assert.Equal("K", classModel.GenericParameters[2].Name);
        Assert.Equal("", classModel.GenericParameters[2].Modifier);
    }

    [Theory]
    [FileData("TestData/DelegateWithMultipleGenericParamsWithModifiers.txt")]
    public void Extract_ShouldHaveGenericModifiers_WhenProvidedWithGenericDelegate(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (DelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1<out T, in TR, in TK>", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("out", classModel.GenericParameters[0].Modifier);
        Assert.Equal("TR", classModel.GenericParameters[1].Name);
        Assert.Equal("in", classModel.GenericParameters[1].Modifier);
        Assert.Equal("TK", classModel.GenericParameters[2].Name);
        Assert.Equal("in", classModel.GenericParameters[2].Modifier);
    }

    [Theory]
    [FileData("TestData/GenericTypeWithPredefinedConstrains.txt")]
    public void Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithDelegate(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel1 = (DelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1<out T, in TK, in TR, in TP>", classModel1.Name);
        Assert.Equal(4, classModel1.GenericParameters.Count);
        Assert.Equal("T", classModel1.GenericParameters[0].Name);
        Assert.Equal(1, classModel1.GenericParameters[0].Constraints.Count);
        Assert.Equal("struct", classModel1.GenericParameters[0].Constraints[0].Name);

        Assert.Equal("TK", classModel1.GenericParameters[1].Name);
        Assert.Equal(1, classModel1.GenericParameters[1].Constraints.Count);
        Assert.Equal("class?", classModel1.GenericParameters[1].Constraints[0].Name);
        Assert.Equal("class", classModel1.GenericParameters[1].Constraints[0].FullType.Name);
        Assert.True(classModel1.GenericParameters[1].Constraints[0].FullType.IsNullable);

        Assert.Equal("TR", classModel1.GenericParameters[2].Name);
        Assert.Equal(1, classModel1.GenericParameters[2].Constraints.Count);
        Assert.Equal("notnull", classModel1.GenericParameters[2].Constraints[0].Name);

        Assert.Equal("TP", classModel1.GenericParameters[3].Name);
        Assert.Equal(1, classModel1.GenericParameters[3].Constraints.Count);
        Assert.Equal("Namespace1.IInterface2<T, Namespace1.IInterface2<T, TK>>",
            classModel1.GenericParameters[3].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2", classModel1.GenericParameters[3].Constraints[0].FullType.Name);
        Assert.Equal(2, classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
        Assert.Equal("Namespace1.IInterface2",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
        Assert.Equal(2,
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
        Assert.Equal("T",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0].Name);
        Assert.Equal("TK",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1].Name);
    }

    [Theory]
    [FileData("TestData/GenericTypeWithMultipleConstrains.txt")]
    public void Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithDelegate(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (DelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1<out T, in TR, in TK>", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal(2, classModel.GenericParameters[0].Constraints.Count);
        Assert.Equal("Namespace1.IInterface", classModel.GenericParameters[0].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2<TK, TR>", classModel.GenericParameters[0].Constraints[1].Name);
        Assert.Equal("Namespace1.IInterface2", classModel.GenericParameters[0].Constraints[1].FullType.Name);
        Assert.Equal(2, classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
        Assert.Equal("TK", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
        Assert.Equal("TR", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
    }
}
