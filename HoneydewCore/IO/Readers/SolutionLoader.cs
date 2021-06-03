using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public class SolutionLoader : ISolutionLoader
    {
        public string ProjectPath { get; private set; }

        private readonly IFileReader _fileReader;
        private IList<PathFilter> _filters;

        public SolutionLoader(IFileReader fileReader)
        {
            _fileReader = fileReader;
        }

        public SolutionLoader()
        {
            _fileReader = new FileReader();
        }

        public SolutionModel LoadSolution(string projectPath)
        {
            _filters ??= new List<PathFilter>();

            ProjectPath = projectPath;

            var filePaths = _fileReader.ReadFilePaths(projectPath);

            if (filePaths.Count == 0)
            {
                throw new ProjectNotFoundException("Project not found at specified Path");
            }

            SolutionModel solutionModel = new();

            foreach (string path in filePaths)
            {
                if (_filters.Count > 0 && !_filters.Any(filter => filter(path)))
                {
                    continue;
                }

                var fileContent = _fileReader.ReadFile(path);
            }

            return solutionModel;
        }

        public SolutionModel LoadSolution(string projectPath, IList<PathFilter> filters)
        {
            _filters = filters;
            return LoadSolution(projectPath);
        }
    }
}