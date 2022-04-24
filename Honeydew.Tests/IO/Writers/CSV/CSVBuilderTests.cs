using System;
using Honeydew.IO.Writers.CSV;
using Xunit;

namespace Honeydew.Tests.IO.Writers.CSV;

public class CsvBuilderTests
{
    private readonly CsvBuilder _sut;

    public CsvBuilderTests()
    {
        _sut = new CsvBuilder();
    }

    [Fact]
    public void AddLine_ShouldThrowInvalidCSVLineLengthException_WhenNoHeaderIsProvided_ButValuesAreAdded()
    {
        Assert.Throws<InvalidCsvLineLengthException>(() => _sut.AddLine(new[] {"Value", "Value1"}));
    }

    [Fact]
    public void AddLine_ShouldThrowInvalidCSVLineLengthException_WhenHeaderIsProvided_ButMoreValuesAreAdded()
    {
        _sut.AddHeader(new[] {"H1", "H2"});
        Assert.Throws<InvalidCsvLineLengthException>(() => _sut.AddLine(new[] {"Value", "Value1", "Value2"}));
    }

    [Fact]
    public void AddLine_ShouldThrowInvalidCSVLineLengthException_WhenHeaderIsProvided_ButFewerValuesAreAdded()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3", "H4", "h4"});
        Assert.Throws<InvalidCsvLineLengthException>(() => _sut.AddLine(new[] {"Value", "Value1", "Value2"}));
    }

    [Fact]
    public void CreateCSV_ShouldReturnEmptyString_WhenNoHeadersAndNoValuesAreProvided()
    {
        Assert.Equal("", _sut.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldReturnCsvStringWithHeaders_WhenHeadersAreProvided()
    {
        Assert.Equal(@"""H1"",""H2"",""H3""", new CsvBuilder(new[] {"H1", "H2", "H3"}).CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldCsvString_WhenHeadersAndValuesAreProvided()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3"});
        _sut.AddLine(new[] {"V1", "V2", "V3"});
        _sut.AddLine(new[] {"X1", "X2", "X3"});

        var newLine = Environment.NewLine;

        var expectedCsv = $@"""H1"",""H2"",""H3""{newLine}""V1"",""V2"",""V3""{newLine}""X1"",""X2"",""X3""";
        Assert.Equal(expectedCsv, _sut.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldReturnCsvStringWithHeaders_WhenHeadersAreProvided_WithOtherSeparator()
    {
        var csvBuilder = new CsvBuilder(new[] {"H1", "H2", "H3"}, ';');
        Assert.Equal(@"""H1"";""H2"";""H3""", csvBuilder.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldCsvString_WhenHeadersAndValuesAreProvided_WithOtherSeparator()
    {
        var csvBuilder = new CsvBuilder(';');
        csvBuilder.AddHeader(new[] {"H1", "H2", "H3"});
        csvBuilder.AddLine(new[] {"V1", "V2", "V3"});
        csvBuilder.AddLine(new[] {"X1", "X2", "X3"});

        var newLine = Environment.NewLine;

        var expectedCsv = $@"""H1"";""H2"";""H3""{newLine}""V1"";""V2"";""V3""{newLine}""X1"";""X2"";""X3""";
        Assert.Equal(expectedCsv, csvBuilder.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldAddTheRightAmountOfColumns_WhenProvidedLessColumnsThanRowCount()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3"});
        _sut.AddLine(new[] {"V1", "V2", "V3"});
        _sut.AddLine(new[] {"X1", "X2", "X3"});
        _sut.AddColumn(new[] {"H4", "V4"});

        var newLine = Environment.NewLine;

        var expectedCsv =
            $@"""H1"",""H2"",""H3"",""H4""{newLine}""V1"",""V2"",""V3"",""V4""{newLine}""X1"",""X2"",""X3""";
        Assert.Equal(expectedCsv, _sut.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldAddTheRightAmountOfColumns_WhenProvidedMoreColumnsThanRowCount()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3"});
        _sut.AddLine(new[] {"V1", "V2", "V3"});
        _sut.AddLine(new[] {"X1", "X2", "X3"});
        _sut.AddColumn(new[] {"H4", "V4", "X4", "Y4"});

        var newLine = Environment.NewLine;

        var expectedCsv =
            $@"""H1"",""H2"",""H3"",""H4""{newLine}""V1"",""V2"",""V3"",""V4""{newLine}""X1"",""X2"",""X3"",""X4""";
        Assert.Equal(expectedCsv, _sut.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldAddTheRightAmountOfColumns_WhenProvidedTheSameAmountOfColumnsAsRowCount()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3"});
        _sut.AddLine(new[] {"V1", "V2", "V3"});
        _sut.AddLine(new[] {"X1", "X2", "X3"});
        _sut.AddColumn(new[] {"H4", "V4", "X4"});

        var newLine = Environment.NewLine;

        var expectedCsv =
            $@"""H1"",""H2"",""H3"",""H4""{newLine}""V1"",""V2"",""V3"",""V4""{newLine}""X1"",""X2"",""X3"",""X4""";
        Assert.Equal(expectedCsv, _sut.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldAddColumn_WhenProvidedAFormulaForEachRow()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3"});
        _sut.AddLine(new[] {"V1", "V2", "V3"});
        _sut.AddLine(new[] {"X1", "X2", "X3"});
        _sut.AddColumnWithFormulaForEachRow(line => line.Contains("H") ? "H4" : line.Length.ToString());

        var newLine = Environment.NewLine;

        var expectedCsv =
            $@"""H1"",""H2"",""H3"",""H4""{newLine}""V1"",""V2"",""V3"",""14""{newLine}""X1"",""X2"",""X3"",""14""";
        Assert.Equal(expectedCsv, _sut.CreateCsv());
    }

    [Fact]
    public void CreateCSV_ShouldAddColumn_WhenProvidedAFormulaForEachRowWithHeaderName()
    {
        _sut.AddHeader(new[] {"H1", "H2", "H3"});
        _sut.AddLine(new[] {"V1", "V2", "V3"});
        _sut.AddLine(new[] {"X1", "X2", "X3"});
        _sut.AddColumnWithFormulaForEachRow("H4", line => line.Length.ToString());

        var newLine = Environment.NewLine;

        var expectedCsv =
            $@"""H1"",""H2"",""H3"",""H4""{newLine}""V1"",""V2"",""V3"",""14""{newLine}""X1"",""X2"",""X3"",""14""";
        Assert.Equal(expectedCsv, _sut.CreateCsv());
    }
}
