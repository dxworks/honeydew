using System;
using System.Collections.Generic;
using System.IO;

namespace HoneydewCore.IO.Readers
{
     public class FileReader : IFileReader
     {
          public string ReadFile(string path)
          {
               return File.ReadAllText(path);
          }

          public IList<string> ReadFilePaths(string directoryPath)
          {
               try
               {
                    return Directory.GetFiles(directoryPath);
               }
               catch (Exception e)
               {
                    Console.WriteLine(e);
                    return new List<string>();
               }
          }
     }
}
