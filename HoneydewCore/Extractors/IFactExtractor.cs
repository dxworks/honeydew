using HoneydewCore.Models;

namespace HoneydewCore.Extractors
{
    public interface IFactExtractor
    {
        string FileType();

        CompilationUnitModel Extract(string fileContent);
    }
}