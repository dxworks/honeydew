using System;
using System.Text.Json;
using System.Threading.Tasks;
using HoneydewModels;

namespace HoneydewCore.IO.Readers.SolutionRead
{
    public class RawFileSolutionLoader : ISolutionLoader
    {
        private readonly IFileReader _fileReader;

        public RawFileSolutionLoader(IFileReader fileReader)
        {
            _fileReader = fileReader;
        }

        public Task<SolutionModel> LoadSolution(string pathToFile)
        {
            var fileContent = _fileReader.ReadFile(pathToFile);

            try
            {
                var solutionModel = JsonSerializer.Deserialize<SolutionModel>(fileContent);

                if (solutionModel == null)
                {
                    return null;
                }

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

                return Task.FromResult(solutionModel);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
