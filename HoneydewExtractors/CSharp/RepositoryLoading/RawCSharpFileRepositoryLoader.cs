using System;
using System.Threading.Tasks;
using HoneydewCore.IO.Readers;
using HoneydewCore.Logging;
using HoneydewModels.CSharp;
using HoneydewModels.Importers;

namespace HoneydewExtractors.CSharp.RepositoryLoading
{
    public class RawCSharpFileRepositoryLoader : IRepositoryLoader<RepositoryModel>
    {
        private readonly ILogger _logger;
        private readonly IFileReader _fileReader;
        private readonly IModelImporter<RepositoryModel> _modelImporter;

        public RawCSharpFileRepositoryLoader(ILogger logger, IFileReader fileReader,
            IModelImporter<RepositoryModel> modelImporter)
        {
            _logger = logger;
            _fileReader = fileReader;
            _modelImporter = modelImporter;
        }

        public Task<RepositoryModel> Load(string path)
        {
            _logger.Log($"Opening File at {path}");

            var fileContent = _fileReader.ReadFile(path);

            try
            {
                var repositoryModel = _modelImporter.Import(fileContent);

                if (repositoryModel == null)
                {
                    return null;
                }

                _logger.Log("Model Loaded");

                return Task.FromResult(repositoryModel);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
