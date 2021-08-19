namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IVisitorType<in TSyntaxNode, TModelType>
    {
        TModelType Visit(TSyntaxNode syntaxNode, TModelType modelType);
    }
}
