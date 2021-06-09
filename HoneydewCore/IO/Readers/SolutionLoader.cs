﻿using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public class SolutionLoader : ISolutionLoader
    {
        private readonly IFileReader _fileReader;
        private readonly IList<IFactExtractor> _extractors;

        public SolutionLoader(IFileReader fileReader, IList<IFactExtractor> extractors)
        {
            _fileReader = fileReader;
            _extractors = extractors;
        }

        public SolutionModel LoadSolution(string projectPath, ISolutionLoadingStrategy strategy)
        {
            var filePaths = _fileReader.ReadFilePaths(projectPath);

            if (filePaths.Count == 0)
            {
                throw new ProjectNotFoundException("Project not found at specified Path");
            }

            SolutionModel solutionModel = new();

            foreach (var path in filePaths)
            {
                var fileContent = _fileReader.ReadFile(path);
                var classModels = strategy.Load(fileContent, _extractors);

                foreach (var classModel in classModels)
                {
                    solutionModel.Add(classModel);
                }
            }

            return solutionModel;
        }
    }
}