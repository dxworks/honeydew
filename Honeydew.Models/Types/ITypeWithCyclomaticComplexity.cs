namespace Honeydew.Models.Types;

public interface ITypeWithCyclomaticComplexity : IType
{
    public int CyclomaticComplexity { get; set; }
}
