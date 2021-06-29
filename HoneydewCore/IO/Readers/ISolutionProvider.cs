using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers
{
    public interface ISolutionProvider
    {
        Solution GetSolution(string path);
    }
}