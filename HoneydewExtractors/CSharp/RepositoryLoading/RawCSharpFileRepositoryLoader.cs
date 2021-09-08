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
        private readonly JsonModelImporter<RepositoryModel> _jsonModelImporter;

        public RawCSharpFileRepositoryLoader(ILogger logger, JsonModelImporter<RepositoryModel> jsonModelImporter)
        {
            _logger = logger;
            _jsonModelImporter = jsonModelImporter;
        }

        public Task<RepositoryModel> Load(string path)
        {
            _logger.Log($"Opening File at {path}");

            try
            {
                var repositoryModel = _jsonModelImporter.Import(path);

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
