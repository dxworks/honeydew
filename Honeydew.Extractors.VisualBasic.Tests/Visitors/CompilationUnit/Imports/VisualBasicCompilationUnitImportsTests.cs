using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.CompilationUnit.Imports;

public class VisualBasicCompilationUnitImportsTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicCompilationUnitImportsTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new ImportsVisitor()
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/CompilationUnitWithImports.txt")]
    public async Task Extract_ShouldImports_WhenProvidedWithCompilationUnit(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        
        Assert.Equal(3, compilationUnitType.Imports.Count);
        var importType1 = compilationUnitType.Imports[0];
        Assert.Equal("System.Text", importType1.Name);
        Assert.Equal("", importType1.Alias);
        Assert.Equal("None", importType1.AliasType);
        Assert.False(importType1.IsStatic);
        
        var importType2 = compilationUnitType.Imports[1];
        Assert.Equal("System.IO", importType2.Name);
        Assert.Equal("", importType2.Alias);
        Assert.Equal("None", importType2.AliasType);
        Assert.False(importType2.IsStatic);
        
        var importType3 = compilationUnitType.Imports[2];
        Assert.Equal("Microsoft.VisualBasic.ControlChars", importType3.Name);
        Assert.Equal("", importType3.Alias);
        Assert.Equal("None", importType3.AliasType);
        Assert.False( importType3.IsStatic);
    }

    [Theory]
    [FilePath("TestData/CompilationUnitWithImportsWithAlias.txt")]
    public async Task Extract_ShouldHaveImportsWithAlias_WhenProvidedWithCompilationUnit(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        Assert.Equal(3, compilationUnitType.Imports.Count);
        var importType1 = compilationUnitType.Imports[0];
        Assert.Equal("System.Text", importType1.Name);
        Assert.Equal("systxt", importType1.Alias);
        Assert.Equal("Namespace", importType1.AliasType);
        Assert.False(importType1.IsStatic);
        
        var importType2 = compilationUnitType.Imports[1];
        Assert.Equal("System.IO", importType2.Name);
        Assert.Equal("sysio", importType2.Alias);
        Assert.Equal("Namespace", importType2.AliasType);
        Assert.False(importType2.IsStatic);
        
        var importType3 = compilationUnitType.Imports[2];
        Assert.Equal("Microsoft.VisualBasic.ControlChars", importType3.Name);
        Assert.Equal("ch", importType3.Alias);
        Assert.Equal("Namespace", importType3.AliasType);
        Assert.False( importType3.IsStatic);
    }
}
