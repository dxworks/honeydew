namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IExtractionVisitor<in TSyntacticModel, in TSemanticModel> : ITypeVisitor
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
    {
        void SetSyntacticModel(TSyntacticModel syntacticModel);

        void SetSemanticModel(TSemanticModel semanticModel);
    }
}
