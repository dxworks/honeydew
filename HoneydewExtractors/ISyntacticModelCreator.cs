namespace HoneydewExtractors
{
    public interface ISyntacticModelCreator
    {
        ISyntacticModel Create(string fileContent);
    }
}
