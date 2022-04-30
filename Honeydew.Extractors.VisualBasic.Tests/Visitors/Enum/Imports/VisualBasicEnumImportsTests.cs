using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Enum.Imports;

public class VisualBasicEnumImportsTests
{
      private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicEnumImportsTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicEnumSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IEnumType>>
                {
                    new BaseInfoEnumVisitor(),
                    new ImportsVisitor(),
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/EnumWithImports.txt")]
    public async Task Extract_ShouldImports_WhenProvidedWithCompilationUnit(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = compilationUnitType.ClassTypes[0];
        Assert.Equal(3, classType.Imports.Count);
        var importType1 = classType.Imports[0];
        Assert.Equal("System.Text", importType1.Name);
        Assert.Equal("", importType1.Alias);
        Assert.Equal("None", importType1.AliasType);
        Assert.False(importType1.IsStatic);
        
        var importType2 = classType.Imports[1];
        Assert.Equal("System.IO", importType2.Name);
        Assert.Equal("", importType2.Alias);
        Assert.Equal("None", importType2.AliasType);
        Assert.False(importType2.IsStatic);
        
        var importType3 = classType.Imports[2];
        Assert.Equal("Microsoft.VisualBasic.ControlChars", importType3.Name);
        Assert.Equal("", importType3.Alias);
        Assert.Equal("None", importType3.AliasType);
        Assert.False( importType3.IsStatic);
    }

    [Theory]
    [FilePath("TestData/EnumWithImportsWithAlias.txt")]
    public async Task Extract_ShouldHaveImportsWithAlias_WhenProvidedWithCompilationUnit(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = compilationUnitType.ClassTypes[0];
        Assert.Equal(3, classType.Imports.Count);
        var importType1 = classType.Imports[0];
        Assert.Equal("System.Text", importType1.Name);
        Assert.Equal("systxt", importType1.Alias);
        Assert.Equal("Namespace", importType1.AliasType);
        Assert.False(importType1.IsStatic);
        
        var importType2 = classType.Imports[1];
        Assert.Equal("System.IO", importType2.Name);
        Assert.Equal("sysio", importType2.Alias);
        Assert.Equal("Namespace", importType2.AliasType);
        Assert.False(importType2.IsStatic);
        
        var importType3 = classType.Imports[2];
        Assert.Equal("Microsoft.VisualBasic.ControlChars", importType3.Name);
        Assert.Equal("ch", importType3.Alias);
        Assert.Equal("Namespace", importType3.AliasType);
        Assert.False( importType3.IsStatic);
    }
}
