﻿using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public class SolutionLoader : ISolutionLoader
    {
        private readonly IFileReader _fileReader;
        private readonly IList<Extractor<IMetricExtractor>> _extractors;

        public SolutionLoader(IFileReader fileReader, IList<Extractor<IMetricExtractor>> extractors)
        {
            _fileReader = fileReader;
            _extractors = extractors;
        }

        public SolutionModel LoadSolution(string projectPath)
        {
            var filePaths = _fileReader.ReadFilePaths(projectPath);

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