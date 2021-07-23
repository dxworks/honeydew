namespace HoneydewExtractors
{
    public interface ISyntacticModelCreator<out TSyntacticModel>
        where TSyntacticModel : ISyntacticModel
    {
        TSyntacticModel Create(string fileContent);
    }
}
