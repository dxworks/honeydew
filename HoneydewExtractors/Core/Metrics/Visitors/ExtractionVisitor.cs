namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public abstract class
        ExtractionVisitor<TSyntacticModel, TSemanticModel> : IExtractionVisitor<TSyntacticModel, TSemanticModel>
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
    {
        protected TSyntacticModel InheritedSyntacticModel;
        protected TSemanticModel InheritedSemanticModel;

        public void SetSyntacticModel(TSyntacticModel syntacticModel)
        {
            InheritedSyntacticModel = syntacticModel;
        }

        public void SetSemanticModel(TSemanticModel semanticModel)
        {
            InheritedSemanticModel = semanticModel;
        }
    }
}
