using Honeydew.Extractors.Visitors;

namespace Honeydew.PostExtraction;

public interface IModelVisitor<TModelType> : ITypeVisitor<TModelType>
{
    void Visit(TModelType modelType);
}
