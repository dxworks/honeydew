using HoneydewCore.Models;

namespace HoneydewCore.Extractors
{
    public interface IExtractor
    {
        public ProjectEntity Extract(string fileContent);
    }
}