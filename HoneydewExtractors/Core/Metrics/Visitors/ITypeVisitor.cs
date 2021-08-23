namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface ITypeVisitor
    {
        void Accept(IVisitor visitor);
    }
}
