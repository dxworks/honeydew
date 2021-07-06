using System;
using System.Text.Json;
using System.Threading.Tasks;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public class RawFileRepositoryLoader : IRepositoryLoader
    {
        private readonly IFileReader _fileReader;

        public RawFileRepositoryLoader(IFileReader fileReader)
        {
            _fileReader = fileReader;
        }

        public Task<RepositoryModel> Load(string path)
        {
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
                        foreach (var (_, projectNamespace) in projectModel.Namespaces)
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

                return Task.FromResult(repositoryModel);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}