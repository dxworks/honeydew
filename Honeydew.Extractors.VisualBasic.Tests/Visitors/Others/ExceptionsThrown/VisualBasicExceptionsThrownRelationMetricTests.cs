using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Others.ExceptionsThrown;

public class VisualBasicExceptionsThrownRelationMetricTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    
    public VisualBasicExceptionsThrownRelationMetricTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new ExceptionsThrownRelationVisitor(),
                    })
            });
    
        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }
    
    [Theory]
    [FilePath("TestData/ClassWithNoExceptionsThrown.txt")]
    public async Task Extract_ShouldHaveNoExceptionsThrown_WhenProvidedWithClassThatDoesntThrowExceptions(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
        Assert.Empty((IDictionary<string, int>)classModel.Metrics[0].Value!);
    }
    
    [Theory]
    [FilePath("TestData/ClassThrowsSystemExceptions.txt")]
    public async Task Extract_ShouldHaveSystemExceptionsThrown_WhenProvidedWithClassThatThrowsSystemExceptions(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
    
        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value!;
    
        Assert.Equal(3, dependencies.Count);
        Assert.Equal(1, dependencies["System.ArgumentNullException"]);
        Assert.Equal(1, dependencies["System.ArgumentException"]);
        Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
    }
    
    
    [Theory]
    [FilePath("TestData/ClassThrowsCustomExceptions.txt")]
    public async Task Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatThrowsCustomExceptions(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[3];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
    
        var dependencies = (IDictionary<string, int>)classModel.Metrics[0].Value!;
    
        Assert.Equal(3, dependencies.Count);
        Assert.Equal(1, dependencies["Throwing.MyArgumentNullException"]);
        Assert.Equal(1, dependencies["Throwing.MyArgumentException"]);
        Assert.Equal(1, dependencies["Throwing.MyIndexOutOfRangeException"]);
    }
    
    [Theory]
    [FilePath("TestData/ClassRethrowsExplicitExceptions.txt")]
    public async Task Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatRethrowsExplicitExceptions(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
    
        var dependencies = (IDictionary<string, int>)classModel.Metrics[0].Value!;
    
        Assert.Equal(2, dependencies.Count);
        Assert.Equal(2, dependencies["Throwing.MyArgumentException"]);
        Assert.Equal(1, dependencies["System.NullReferenceException"]);
    }
    
    [Theory]
    [FilePath("TestData/ClassRethrowsImplicitExceptions.txt")]
    public async Task Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatRethrowsImplicitExceptions(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
    
        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value!;
    
        Assert.Equal(2, dependencies.Count);
        Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
        Assert.Equal(1, dependencies["System.NullReferenceException"]);
    }
    
    [Theory]
    [FilePath("TestData/ClassThrowsExceptionsUsingVariablesParametersFieldsPropertiesAndMethodCalls.txt")]
    public async Task
        Extract_ShouldHaveExceptionsThrown_WhenProvidedWithClassThatTrowsExceptionsUsingVariablesParametersFieldsPropertiesAndMethodCalls(
            string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
    
        var dependencies = (IDictionary<string, int>)classModel.Metrics[0].Value!;
    
        Assert.Equal(6, dependencies.Count);
        Assert.Equal(1, dependencies["System.IndexOutOfRangeException"]);
        Assert.Equal(1, dependencies["System.NullReferenceException"]);
        Assert.Equal(1, dependencies["System.Exception"]);
        Assert.Equal(1, dependencies["ExternException"]);
        Assert.Equal(1, dependencies["System.NotSupportedException"]);
        Assert.Equal(2, dependencies["Throwing.MyException"]);
    }
    
    [Theory]
    [FilePath("TestData/ClassThrowsExternalExceptions.txt")]
    public async Task Extract_ShouldHaveExternalExceptionsThrown_WhenProvidedWithClassThatTrowsExternalExceptions(
        string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
    
        Assert.Equal(1, classModel.Metrics.Count);
        Assert.Equal("Honeydew.Extractors.VisualBasic.Visitors.Concrete.ExceptionsThrownRelationVisitor",
            classModel.Metrics[0].ExtractorName);
        Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            classModel.Metrics[0].ValueType);
    
        var dependencies = (Dictionary<string, int>)classModel.Metrics[0].Value!;
    
        Assert.Single(dependencies);
        Assert.Equal(8, dependencies["ExternException"]);
    }
}
