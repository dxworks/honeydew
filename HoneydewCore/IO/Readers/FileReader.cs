using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.IO.Readers.Filters;

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
               return ReadFilePaths(directoryPath, new List<PathFilter>());
          }

          public IList<string> ReadFilePaths(string directoryPath, IList<PathFilter> filters)
          {
               try
               {
                    var filePaths = Directory.GetFiles(directoryPath);

                    if (filters == null || filters.Count == 0)
                    {
                         return filePaths;
                    }

                    return filePaths.Where(path => filters.Any(filter => filter(path))).ToList();
               }
               catch (Exception e)
               {
                    Console.WriteLine(e);
                    return new List<string>();
               }
          }
     }
}
