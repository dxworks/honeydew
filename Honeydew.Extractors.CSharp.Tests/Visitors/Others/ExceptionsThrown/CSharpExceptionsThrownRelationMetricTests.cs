using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Others.ExceptionsThrown;

public class CSharpExceptionsThrownRelationMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpExceptionsThrownRelationMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new ExceptionsThrownRelationVisitor(),
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ClassWithNoExceptionsThrown.txt")]
    public void Extract_ShouldHaveNoExceptionsThrown_WhenProvidedWithClassThatDoesntThrowExceptions(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);
        Assert.Empty((IDictionary<string, int>)classTypes[1].Metrics[0].Value!);
    }

    [Theory]
    [FileData("TestData/ClassThrowsSystemExceptions.txt")]
    public void Extract_ShouldHaveSystemExceptionsThrown_WhenProvidedWithClassThatThrowsSystemExceptions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(1, dependencies["System.ArgumentNullException"]);
        Assert.Equal(1, dependencies["System.ArgumentException"]);
        Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
    }


    [Theory]
    [FileData("TestData/ClassThrowsCustomExceptions.txt")]
    public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatThrowsCustomExceptions(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[3].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[3].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[3].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[3].Metrics[0].Value!;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(1, dependencies["Throwing.MyArgumentNullException"]);
        Assert.Equal(1, dependencies["Throwing.MyArgumentException"]);
        Assert.Equal(1, dependencies["Throwing.MyIndexOutOfRangeException"]);
    }

    [Theory]
    [FileData("TestData/ClassRethrowsExplicitExceptions.txt")]
    public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatRethrowsExplicitExceptions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(2, dependencies["Throwing.MyArgumentException"]);
        Assert.Equal(1, dependencies["System.NullReferenceException"]);
    }

    [Theory]
    [FileData("TestData/ClassRethrowsImplicitExceptions.txt")]
    public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatRethrowsImplicitExceptions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
        Assert.Equal(1, dependencies["System.NullReferenceException"]);
    }

    [Theory]
    [FileData("TestData/ClassThrowsExceptionsUsingVariablesParametersFieldsPropertiesAndMethodCalls.txt")]
    public void
        Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatTrowsExceptionsUsingVariablesParametersFieldsPropertiesAndMethodCalls(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(5, dependencies.Count);
        Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
        Assert.Equal(1, dependencies["System.NullReferenceException"]);
        Assert.Equal(1, dependencies["System.Exception"]);
        Assert.Equal(1, dependencies["System.NotSupportedException"]);
        Assert.Equal(2, dependencies["Throwing.MyException"]);
    }

    [Theory]
    [FileData("TestData/ClassThrowsExternalExceptions.txt")]
    public void Extract_ShouldHaveExternalExceptionsThrown_WhenProvidedWithClassThatTrowsExternalExceptions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Single(dependencies);
        Assert.Equal(8, dependencies["ExternException"]);
    }

    [Theory]
    [FileData("TestData/ClassThrowsExceptionFromConditionalOperator.txt")]
    public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithConditionalOperatorWithThrowException(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Single(dependencies);
        Assert.Equal(1, dependencies["System.ArgumentException"]);
    }

    [Theory]
    [FileData("TestData/ClassThrowsExceptionFromCoalescingOperator.txt")]
    public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithNullCoalescingOperatorWithThrowException(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Single(dependencies);
        Assert.Equal(1, dependencies["System.ArgumentNullException"]);
    }

    [Theory]
    [FileData("TestData/ClassThrowsExceptionFromLambda.txt")]
    public void Extract_ShouldHaveExceptionsThrown_WhenProvidedWithLambdaThaThrowsException(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Single(dependencies);
        Assert.Equal(1, dependencies["System.InvalidCastException"]);
    }
}
