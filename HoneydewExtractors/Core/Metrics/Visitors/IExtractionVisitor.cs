namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IExtractionVisitor<in TSyntaxNode, TModelType> : ITypeVisitor
    {
        TModelType Visit(TSyntaxNode syntaxNode, TModelType modelType);
    }
}
