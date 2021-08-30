namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IVisitor
    {
        void Visit(ITypeVisitor visitor);
    }
}
