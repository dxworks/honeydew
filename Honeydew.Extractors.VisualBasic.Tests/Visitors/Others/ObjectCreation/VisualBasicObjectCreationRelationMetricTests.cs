using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Others.ObjectCreation;

public class VisualBasicObjectCreationRelationMetricTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicObjectCreationRelationMetricTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new ObjectCreationRelationVisitor(),
                    })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/ObjectCreationFromClassInTheSameNamespace.txt")]
    public async Task Extract_ShouldHaveObjectCreation_WhenProvidedWithClassInTheSameNamespace(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(1, dependencies.Count);
        Assert.Equal(7, dependencies["App.C"]);
    }

    [Theory]
    [FilePath("TestData/ArrayCreationFromClassInTheSameNamespace.txt")]
    public async Task Extract_ShouldHaveOArrayCreation_WhenProvidedWithClassInTheSameNamespace(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Single( dependencies);
        Assert.Equal(14, dependencies["App.C"]);
    }

    [Theory]
    [FilePath("TestData/ObjectCreationFromUnknownClass.txt")]
    public async Task Extract_ShouldHaveObjectCreation_WhenProvidedWithClassUnknownClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(2, dependencies.Count);
        Assert.Equal(1, dependencies["ComputedC"]);
        Assert.Equal(6, dependencies["ExternClass"]);
    }

    [Theory]
    [FilePath("TestData/ArrayCreationFromUnknownClass.txt")]
    public async Task Extract_ShouldHaveArrayCreation_WhenProvidedWithClassUnknownClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Single(dependencies);
        Assert.Equal(14, dependencies["ExternClass"]);
    }

    [Theory]
    [FilePath("TestData/ArrayCreationFromPrimitiveTypes.txt")]
    public async Task Extract_ShouldHaveArrayCreation_WhenProvidedWithClassPrimitiveTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[0].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[0].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[0].Metrics[0].ValueType);

        var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value!;

        Assert.Equal(3, dependencies.Count);
        Assert.Equal(2, dependencies["String"]);
        Assert.Equal(1, dependencies["Integer"]);
        Assert.Equal(1, dependencies["Boolean"]);
    }

    [Theory]
    [FilePath("TestData/ArrayCreationWithUnknownClass.txt")]
    public async Task Extract_ShouldHaveArrayCreation_WhenProvidedWithClassInSameNamespaceUsedWithUnknownClassMethod(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        Assert.Equal(1, classTypes[1].Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ObjectCreationRelationVisitor",
            classTypes[1].Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classTypes[1].Metrics[0].ValueType);

        var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value!;

        Assert.Equal(1, dependencies.Count);
        Assert.Equal(4, dependencies["App.Class1"]);
    }
}
