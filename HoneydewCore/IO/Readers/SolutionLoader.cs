using System;
using System.Collections.Generic;
using System.Text.Json;
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
                    classModel.FilePath = path;
                    solutionModel.Add(classModel);
                }
            }

            return solutionModel;
        }

        public SolutionModel LoadModelFromFile(string pathToModel)
        {
            var fileContent = _fileReader.ReadFile(pathToModel);

            try
            {
                var solutionModel = JsonSerializer.Deserialize<SolutionModel>(fileContent);

                if (solutionModel == null)
                {
                    return null;
                }

                foreach (var (_, projectNamespace) in solutionModel.Namespaces)
                {
                    foreach (var classModel in projectNamespace.ClassModels)
                    {
                        foreach (var metric in classModel.Metrics)
                        {
                            var returnType = Type.GetType(metric.ValueType);
                            if (returnType == null)
                            {
                                continue;
                            }
                            
                            metric.Value = JsonSerializer.Deserialize(((JsonElement) metric.Value).GetRawText(),
                                returnType);
                        }
                    }
                }

                return solutionModel;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}