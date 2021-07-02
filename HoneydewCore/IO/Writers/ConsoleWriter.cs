using System;

namespace HoneydewCore.IO.Writers
{
    public class ConsoleWriter : IFileWriter
    {
        public void WriteFile(string filePath, string fileContent)
        {
            Console.WriteLine(fileContent);
        }
    }
}