namespace HoneydewModels.Types
{
    public interface IConstructorType : IMethodSkeletonType
    {
        public LinesOfCode Loc { get; set; }
    }
}
