using System.Collections.Generic;

namespace HoneydewCore.IO.Readers
{
     public interface IFileReader
     {
          string ReadFile(string path);

          IList<string> ReadFilePaths(string directoryPath);
     }
}
