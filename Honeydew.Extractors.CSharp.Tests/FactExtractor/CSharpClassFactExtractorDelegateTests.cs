using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.FactExtractor;

public class CSharpClassFactExtractorDelegateTests
{
    private readonly CSharpFactExtractor _sut;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpClassFactExtractorDelegateTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpDelegateSetterCompilationUnitVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    new BaseInfoDelegateVisitor(),
                    new CSharpParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                    {
                        new ParameterInfoVisitor()
                    })
                })
            });

        _sut = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/MultipleDelegatesWithPrimitiveTypes.txt")]
    public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithPrimitiveTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(3, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var delegateModel = (DelegateModel)classType;
            Assert.Equal("MyDelegates", delegateModel.ContainingNamespaceName);
            Assert.Equal("", delegateModel.ContainingClassName);
            Assert.Equal(1, delegateModel.BaseTypes.Count);
            Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
            Assert.Equal("delegate", delegateModel.ClassType);
            Assert.Equal("public", delegateModel.AccessModifier);
            Assert.Equal("", delegateModel.Modifier);
            Assert.Empty(delegateModel.Metrics);
        }

        var delegateModel0 = (DelegateModel)classTypes[0];
        Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
        Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
        Assert.Empty(delegateModel0.ParameterTypes);

        var delegateModel1 = (DelegateModel)classTypes[1];
        Assert.Equal("MyDelegates.Delegate2", delegateModel1.Name);
        Assert.Equal("void", delegateModel1.ReturnValue.Type.Name);
        Assert.Equal(1, delegateModel1.ParameterTypes.Count);
        var parameterModel1 = (ParameterModel)delegateModel1.ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("string", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var delegateModel2 = (DelegateModel)classTypes[2];
        Assert.Equal("MyDelegates.Delegate3", delegateModel2.Name);
        Assert.Equal("int", delegateModel2.ReturnValue.Type.Name);
        Assert.Equal(2, delegateModel2.ParameterTypes.Count);

        var parameterModel2 = (ParameterModel)delegateModel2.ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("double", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);

        var parameterModel3 = (ParameterModel)delegateModel2.ParameterTypes[1];
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Equal("char", parameterModel3.Type.Name);
        Assert.Null(parameterModel3.DefaultValue);
    }

    [Theory]
    [FileData("TestData/MultipleDelegatesWithClassTypes.txt")]
    public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithOtherClasses(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var delegateModel = (DelegateModel)classType;
            Assert.Equal("MyDelegates", delegateModel.ContainingNamespaceName);
            Assert.Equal("", delegateModel.ContainingClassName);
            Assert.Equal(1, delegateModel.BaseTypes.Count);
            Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
            Assert.Equal("delegate", delegateModel.ClassType);
            Assert.Equal("public", delegateModel.AccessModifier);
            Assert.Equal("", delegateModel.Modifier);
            Assert.Empty(delegateModel.Metrics);
        }

        var delegateModel0 = (DelegateModel)classTypes[0];
        Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
        Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
        Assert.Equal(1, delegateModel0.ParameterTypes.Count);
        var parameterModel1 = (ParameterModel)delegateModel0.ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("MyDelegates.Class1", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var delegateModel1 = (DelegateModel)classTypes[1];
        Assert.Equal("MyDelegates.Delegate2", delegateModel1.Name);
        Assert.Equal("MyDelegates.Class1", delegateModel1.ReturnValue.Type.Name);
        Assert.Equal(1, delegateModel1.ParameterTypes.Count);
        var parameterModel2 = (ParameterModel)delegateModel1.ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("ExternClass", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/DelegateWithParametersWithModifiers.txt")]
    public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithParametersWithModifiers(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var delegateModel0 = (DelegateModel)classTypes[0];
        Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
        Assert.Equal("MyDelegates", delegateModel0.ContainingNamespaceName);
        Assert.Equal("", delegateModel0.ContainingClassName);
        Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
        Assert.Equal(3, delegateModel0.ParameterTypes.Count);

        var parameterModel1 = (ParameterModel)delegateModel0.ParameterTypes[0];
        Assert.Equal("out", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var parameterModel2 = (ParameterModel)delegateModel0.ParameterTypes[1];
        Assert.Equal("in", parameterModel2.Modifier);
        Assert.Equal("string", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);

        var parameterModel3 = (ParameterModel)delegateModel0.ParameterTypes[2];
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Equal("char", parameterModel3.Type.Name);
        Assert.Equal("'a'", parameterModel3.DefaultValue);
    }

    [Theory]
    [FileData("TestData/DelegatesInClasses.txt")]
    public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesInInnerClasses(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(3, classTypes.Count);


        foreach (var classType in classTypes)
        {
            var delegateModel = (DelegateModel)classType;
            Assert.Equal(1, delegateModel.BaseTypes.Count);
            Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
            Assert.Equal("delegate", delegateModel.ClassType);
            Assert.Equal("internal", delegateModel.AccessModifier);
            Assert.Equal("", delegateModel.Modifier);
            Assert.Empty(delegateModel.Metrics);
        }

        var delegateModel0 = (DelegateModel)classTypes[0];
        Assert.Equal("MyDelegates", delegateModel0.ContainingNamespaceName);
        Assert.Equal("", delegateModel0.ContainingClassName);
        Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
        Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
        Assert.Equal(1, delegateModel0.ParameterTypes.Count);
        var parameterModel1 = (ParameterModel)delegateModel0.ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var delegateModel1 = (DelegateModel)classTypes[1];
        Assert.Equal("MyDelegates", delegateModel1.ContainingNamespaceName);
        Assert.Equal("MyDelegates.Class1", delegateModel1.ContainingClassName);
        Assert.Equal("MyDelegates.Class1.Delegate2", delegateModel1.Name);
        Assert.Equal("int", delegateModel1.ReturnValue.Type.Name);
        Assert.Empty(delegateModel1.ParameterTypes);

        var delegateModel2 = (DelegateModel)classTypes[2];
        Assert.Equal("MyDelegates", delegateModel2.ContainingNamespaceName);
        Assert.Equal("MyDelegates.Class1.InnerClass", delegateModel2.ContainingClassName);
        Assert.Equal("MyDelegates.Class1.InnerClass.Delegate3", delegateModel2.Name);
        Assert.Equal("int", delegateModel2.ReturnValue.Type.Name);
        Assert.Equal(1, delegateModel2.ParameterTypes.Count);
        var parameterModel2 = (ParameterModel)delegateModel2.ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("string", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }
}
