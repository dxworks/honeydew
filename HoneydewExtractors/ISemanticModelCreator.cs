namespace HoneydewExtractors
{
    public interface ISemanticModelCreator
    {
        ISemanticModel Create(ISyntacticModel syntacticModel);
    }
}
