using HoneydewModels.Types;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IModelVisitor<TType> : ITypeVisitor
    {
        TType Visit(TType modelType);
    }

    public interface IModelVisitor : IModelVisitor<IType>
    {
    }
}
