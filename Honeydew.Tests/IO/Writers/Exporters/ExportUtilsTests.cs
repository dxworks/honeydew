using Honeydew.IO.Writers.Exporters;
using Xunit;

namespace Honeydew.Tests.IO.Writers.Exporters;

public class ExportUtilsTests
{
    [Fact]
    public void CsvCountPerLine_ShouldReturn0_WhenNoNumberIsDetected()
    {
        const string line = @"""A"",""t2"",""B"",""K2a1"",""BWZ"",""OFA2""";

        var actualResult = ExportUtils.CsvSumPerLine.Invoke(line);
        Assert.Equal("0", actualResult);
    }

    [Fact]
    public void CsvCountPerLine_ShouldReturnTheSumOfNumbers_WhenOnlyNumberAreProvided()
    {
        const string line = @"""2"",""2"",""1"",""3"",""4"",""5""";

        var actualResult = ExportUtils.CsvSumPerLine.Invoke(line);
        Assert.Equal("17", actualResult);
    }

    [Fact]
    public void CsvCountPerLine_ShouldReturnTheSumOfNumbers_WhenNumbersAndStringsAreProvided()
    {
        const string line = @"""a2"",""b2"",""1"",""3x"",""4"",""5d""";

        var actualResult = ExportUtils.CsvSumPerLine.Invoke(line);
        Assert.Equal("5", actualResult);
    }
}
