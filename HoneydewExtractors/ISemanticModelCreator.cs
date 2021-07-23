namespace HoneydewExtractors
{
    public interface ISemanticModelCreator<in TSyntacticModel, out TSemanticModel>
        where TSyntacticModel : ISyntacticModel
        where TSemanticModel : ISemanticModel
    {
        TSemanticModel Create(TSyntacticModel syntacticModel);
    }
}
