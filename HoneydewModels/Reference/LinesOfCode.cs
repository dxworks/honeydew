namespace HoneydewModels.Reference;

public struct LinesOfCode
{
    public int SourceLines { get; set; }

    public int CommentedLines { get; set; }

    public int EmptyLines { get; set; }
}
