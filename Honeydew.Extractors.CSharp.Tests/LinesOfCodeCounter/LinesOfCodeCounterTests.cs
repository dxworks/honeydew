using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.LinesOfCodeCounter;

public class LinesOfCodeCounterTests
{
    private readonly ILinesOfCodeCounter _sut;

    public LinesOfCodeCounterTests()
    {
        _sut = new CSharpLinesOfCodeCounter();
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithOnlySourceLines.txt")]
    public void Count_ShouldReturnSourceLinesCount_WhenProvidedWithContentWithOnlySourceLines(string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(8, linesOfCode.SourceLines);
        Assert.Equal(9, linesOfCode.EmptyLines);
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
        Assert.Equal(16, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithOnlyMultiLineComments.txt")]
    public void Count_ShouldReturnCommentLinesCount_WhenProvidedWithContentWithOnlyMultiLineComments(string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(0, linesOfCode.SourceLines);
        Assert.Equal(1, linesOfCode.EmptyLines);
        Assert.Equal(18, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceLinesAndWhitespaceLines.txt")]
    public void Count_ShouldReturnWhitespaceLinesCount_WhenProvidedWithContentWithSourceAndWhitespaceLines(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(9, linesOfCode.SourceLines);
        Assert.Equal(17, linesOfCode.EmptyLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceAndCommentedAndWhitespaceLines.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndCommentedAndWhitespaceLines(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(11, linesOfCode.SourceLines);
        Assert.Equal(17, linesOfCode.EmptyLines);
        Assert.Equal(8, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceAndCommentLinesOnTheSameLine.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndCommentedOnTheSameLine(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(5, linesOfCode.SourceLines);
        Assert.Equal(4, linesOfCode.EmptyLines);
        Assert.Equal(3, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithSourceAndImbricatedMultiLineComments.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndImbricatedMultilineComments(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(3, linesOfCode.SourceLines);
        Assert.Equal(2, linesOfCode.EmptyLines);
        Assert.Equal(8, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithMultipleMultiLineCommentsOnTheSameLine.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSourceAndMultilineCommentsOnTheSameLine(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(4, linesOfCode.SourceLines);
        Assert.Equal(4, linesOfCode.EmptyLines);
        Assert.Equal(2, linesOfCode.CommentedLines);
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithMultipleCommentsOnTheSameLine.txt")]
    public void Count_ShouldReturnLinesOfCodeCount_WhenProvidedWithContentWithSingleLineAndMultiLineComments(
        string fileContent)
    {
        var linesOfCode = _sut.Count(fileContent);

        Assert.Equal(4, linesOfCode.SourceLines);
        Assert.Equal(5, linesOfCode.EmptyLines);
        Assert.Equal(3, linesOfCode.CommentedLines);
    }
}
