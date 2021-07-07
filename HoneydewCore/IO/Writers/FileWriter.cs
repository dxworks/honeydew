using System.IO;

namespace HoneydewCore.IO.Writers
{
    public class FileWriter : IFileWriter
    {
        public void WriteFile(string filePath, string fileContent)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            File.WriteAllText(filePath, fileContent);
        }
    }
}