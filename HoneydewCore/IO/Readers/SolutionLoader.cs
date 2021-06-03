using System.Collections.Generic;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public class SolutionLoader : ISolutionLoader
    {
        public string ProjectPath { get; private set; }

        private IList<PathFilter> _filters;

        private readonly IFileReader _fileReader;

        public SolutionLoader(IFileReader fileReader, IList<PathFilter> filters)
        {
            _fileReader = fileReader;
            _filters = filters;
        }

        public SolutionLoader()
        {
            _fileReader = new FileReader();
        }

        public void SetPathFilters(IList<PathFilter> filters)
        {
            _filters = filters;
        }

        public SolutionModel LoadSolution(string projectPath)
        {
            ProjectPath = projectPath;

            var filePaths = _fileReader.ReadFilePaths(projectPath, _filters);

            if (filePaths.Count == 0)
            {
                throw new ProjectNotFoundException("Project not found at specified Path");
            }

            SolutionModel solutionModel = new();

            foreach (string path in filePaths)
            {
                var fileContent = _fileReader.ReadFile(path);
            }

            return solutionModel;
        }
    }
}