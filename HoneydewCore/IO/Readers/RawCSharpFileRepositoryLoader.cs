using System;
using System.Text.Json;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors;
using HoneydewModels;

namespace HoneydewCore.IO.Readers
{
    public class RawCSharpFileRepositoryLoader : IRepositoryLoader<RepositoryModel>
    {
        private readonly IProgressLogger _progressLogger;
        private readonly IFileReader _fileReader;

        public RawCSharpFileRepositoryLoader(IProgressLogger progressLogger, IFileReader fileReader)
        {
            _progressLogger = progressLogger;
            _fileReader = fileReader;
        }

        public Task<RepositoryModel> Load(string path)
        {
            _progressLogger.LogLine($"Opening File at {path}");

            var fileContent = _fileReader.ReadFile(path);

            try
            {
                var repositoryModel = JsonSerializer.Deserialize<RepositoryModel>(fileContent);

                if (repositoryModel == null)
                {
                    return null;
                }

                foreach (var solutionModel in repositoryModel.Solutions)
                {
                    foreach (var projectModel in solutionModel.Projects)
                    {
                        foreach (var projectNamespace in projectModel.Namespaces)
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
                    }
                }

                _progressLogger.LogLine("Model Loaded");

                return Task.FromResult(repositoryModel);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
