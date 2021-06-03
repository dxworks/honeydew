using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.IO.Readers.Filters;

namespace HoneydewCore.IO.Readers
{
    public class FileReader : IFileReader
    {
        private readonly IList<PathFilter> _filters;

        public FileReader(IList<PathFilter> filters)
        {
            _filters = filters ?? new List<PathFilter>();
        }
        
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public IList<string> ReadFilePaths(string directoryPath)
        {
            try
            {
                var filePaths = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

                if (_filters.Count == 0)
                {
                    return filePaths;
                }

                return filePaths.Where(path => _filters.Any(filter => filter(path))).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<string>();
            }
        }
    }
}