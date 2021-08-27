namespace HoneydewModels.Types
{
    public interface ITypeWithCyclomaticComplexity : IType
    {
        public int CyclomaticComplexity { get; set; }
    }
}
