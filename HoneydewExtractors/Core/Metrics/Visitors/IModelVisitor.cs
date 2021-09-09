namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IModelVisitor<in TType>
    {
        void Visit(TType modelType);
    }
}
