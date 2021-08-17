namespace HoneydewExtractors.Core
{
    public interface IRepositoryClassSet
    {
        void Add(string projectName, string classFullName);
        bool Contains(string projectName, string className);
    }
}
