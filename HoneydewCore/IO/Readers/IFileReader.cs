using System.Collections.Generic;
using HoneydewCore.IO.Readers.Filters;

namespace HoneydewCore.IO.Readers
{
    public interface IFileReader
    {
        string ReadFile(string path);

        IList<string> ReadFilePaths(string directoryPath);
    }
}