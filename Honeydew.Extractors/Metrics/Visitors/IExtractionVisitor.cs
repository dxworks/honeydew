namespace Honeydew.Extractors.Metrics.Visitors;

public interface IExtractionVisitor<in TSyntaxNode, in TSemanticModel, TModelType> : ITypeVisitor
{
    TModelType Visit(TSyntaxNode syntaxNode, TSemanticModel semanticModel, TModelType modelType);
}
