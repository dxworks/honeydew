using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.LinesOfCodeCounter;

public class VisualBasicLinesOfCodeCounterTests
{
    private readonly ILinesOfCodeCounter _sut;

    public VisualBasicLinesOfCodeCounterTests()
    {
        _sut = new VisualBasicLinesOfCodeCounter();
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithOnlySourceLines.txt")]
    public void Count_ShouldReturnSourceLinesCount_WhenProvidedWithContentWithOnlySourceLines(string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(13, linesOfCode.SourceLines);
        Assert.Equal(1, linesOfCode.EmptyLines);
        Assert.Equal(0, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithOnlySingleCommentedLines.txt")]
    public void Count_ShouldReturnCommentLinesCount_WhenProvidedWithContentWithOnlySingleLineComments(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(0, linesOfCode.SourceLines);
        Assert.Equal(1, linesOfCode.EmptyLines);
        Assert.Equal(13, linesOfCode.CommentedLines);
    }
    

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceLinesAndWhitespaceLines.txt")]
    public void Count_ShouldReturnWhitespaceLinesCount_WhenProvidedWithContentWithSourceAndWhitespaceLines(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(13, linesOfCode.SourceLines);
        Assert.Equal(7, linesOfCode.EmptyLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceAndCommentedAndWhitespaceLines.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndCommentedAndWhitespaceLines(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(15, linesOfCode.SourceLines);
        Assert.Equal(8, linesOfCode.EmptyLines);
        Assert.Equal(7, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceAndCommentLinesOnTheSameLine.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndCommentedOnTheSameLine(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(8, linesOfCode.SourceLines);
        Assert.Equal(1, linesOfCode.EmptyLines);
        Assert.Equal(4, linesOfCode.CommentedLines);
    }
}
