namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public abstract class
        ExtractionVisitor<TSyntacticModel, TSemanticModel> : ITypeVisitor
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
    {
        public TSyntacticModel InheritedSyntacticModel { get; set; }
        public TSemanticModel InheritedSemanticModel { get; set; }

        public virtual void Accept(IVisitor visitor)
        {
        }
    }
}
