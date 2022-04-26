using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Others.ObjectCreation;

public class CSharpObjectCreationRelationMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpObjectCreationRelationMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new ObjectCreationRelationVisitor(_loggerMock.Object),
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ObjectCreationFromClassInTheSameNamespace.txt")]
    public void Extract_ShouldHaveObjectCreation_WhenProvidedWithClassInTheSameNamespace(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(1, dependencies.Count);
        Assert.Equal(8, dependencies["App.C"]);
    }

    [Theory]
    [FileData("TestData/ArrayCreationFromClassInTheSameNamespace.txt")]
    public void Extract_ShouldHaveOArrayCreation_WhenProvidedWithClassInTheSameNamespace(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(8, dependencies["App.C[]"]);
        Assert.Equal(1, dependencies["App.C[2]"]);
        Assert.Equal(10, dependencies["App.C"]);
    }


    [Theory]
    [FileData("TestData/ObjectCreationFromUnknownClass.txt")]
    public void Extract_ShouldHaveObjectCreation_WhenProvidedWithClassUnknownClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Single(dependencies);
        Assert.Equal(8, dependencies["ExternClass"]);
    }

    [Theory]
    [FileData("TestData/ArrayCreationFromUnknownClass.txt")]
    public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassUnknownClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(8, dependencies["ExternClass[]"]);
        Assert.Equal(1, dependencies["ExternClass[2]"]);
        Assert.Equal(10, dependencies["ExternClass"]);
    }

    [Theory]
    [FileData("TestData/ArrayCreationFromPrimitiveTypes.txt")]
    public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassPrimitiveTypes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(5, dependencies.Count);
        Assert.Equal(3, dependencies["string[]"]);
        Assert.Equal(2, dependencies["int[]"]);
        Assert.Equal(1, dependencies["int[2]"]);
        Assert.Equal(2, dependencies["double[]"]);
        Assert.Equal(1, dependencies["bool[]"]);
    }

    [Theory]
    [FileData("TestData/ArrayCreationFromPrimitiveTypesInUnknownClassMethod.txt")]
    public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassPrimitiveTypesInUnknownClassMethod(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(6, dependencies.Count);
        Assert.Equal(1, dependencies["string[]"]);
        Assert.Equal(1, dependencies["double[]"]);
        Assert.Equal(1, dependencies["float[]"]);
        Assert.Equal(1, dependencies["int[]"]);
        Assert.Equal(1, dependencies["bool[]"]);
        Assert.Equal(1, dependencies["System.Object[]"]);
    }

    [Theory]
    [FileData("TestData/ArrayCreationForLocalVariables.txt")]
    public void
        Extract_ShouldHaveArrayCreation_WhenProvidedWithArrayCreationOfLocalVariablesPropertiesFieldsAndMethodCallsInUnknownClassMethod(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(4, dependencies.Count);
        Assert.Equal(1, dependencies["string[]"]);
        Assert.Equal(1, dependencies["double[]"]);
        Assert.Equal(1, dependencies["int[]"]);
        Assert.Equal(1, dependencies["bool[]"]);
    }

    [Theory]
    [FileData("TestData/ArrayCreationWithUnknownClass.txt")]
    public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassInSameNamespaceUsedWithUnknownClassMethod(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.CSharp.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(4, dependencies["App.Class1"]);
        Assert.Equal(2, dependencies["App.Class1[]"]);
        Assert.Equal(1, dependencies["System.Object[]"]);
    }
}
