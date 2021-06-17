using System.IO;

namespace HoneydewCore.IO.Readers
{
    public class FileReader : IFileReader
    {
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}