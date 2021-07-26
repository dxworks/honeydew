using System;
using System.Threading.Tasks;
using HoneydewCore.IO.Readers;
using HoneydewModels.CSharp;
using HoneydewModels.Importers;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public class RawFileSolutionLoader : ISolutionLoader
    {
        private readonly IFileReader _fileReader;
        private readonly IModelImporter<SolutionModel> _modelImporter;

        public RawFileSolutionLoader(IFileReader fileReader, IModelImporter<SolutionModel> modelImporter)
        {
            _fileReader = fileReader;
            _modelImporter = modelImporter;
        }

        public Task<SolutionModel> LoadSolution(string pathToFile)
        {
            var fileContent = _fileReader.ReadFile(pathToFile);

            try
            {
                var solutionModel = _modelImporter.Import(fileContent);

                return solutionModel == null ? null : Task.FromResult(solutionModel);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
