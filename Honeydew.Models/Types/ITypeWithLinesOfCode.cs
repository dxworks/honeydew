namespace Honeydew.Models.Types;

public interface ITypeWithLinesOfCode : IType
{
    public LinesOfCode Loc { get; set; }
}
