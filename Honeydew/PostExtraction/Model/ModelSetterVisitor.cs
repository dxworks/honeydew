using Honeydew.Extractors.Visitors;

namespace Honeydew.PostExtraction.Model;

public abstract class ModelSetterVisitor<TSetterType, TWrappedType> : IModelVisitor<TSetterType>
{
    private readonly IEnumerable<IModelVisitor<TWrappedType>> _visitors;

    protected ModelSetterVisitor(IEnumerable<IModelVisitor<TWrappedType>> visitors)
    {
        _visitors = visitors;
    }

    protected IEnumerable<ITypeVisitor<TWrappedType>> GetContainedVisitors()
    {
        return _visitors;
    }

    public abstract void Visit(TSetterType modelType);
}
