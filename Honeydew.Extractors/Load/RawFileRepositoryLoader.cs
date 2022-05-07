using Honeydew.Extractors.Importers;
using Honeydew.Logging;
using Honeydew.Models;

namespace Honeydew.Extractors.Load;

public class RawFileRepositoryLoader
{
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly RepositoryModelImporter _repositoryModelImporter;

    public RawFileRepositoryLoader(ILogger logger, IProgressLogger progressLogger,
        RepositoryModelImporter repositoryModelImporter)
    {
        _logger = logger;
        _repositoryModelImporter = repositoryModelImporter;
        _progressLogger = progressLogger;
    }

    public async Task<RepositoryModel?> Load(string path, CancellationToken cancellationToken)
    {
        _logger.Log($"Opening File at {path}");
        _progressLogger.Log($"Opening File at {path}");

        try
        {
            var repositoryModel = await _repositoryModelImporter.Import(path, cancellationToken);

            if (repositoryModel == null)
            {
                return null;
            }

            _logger.Log("Model Loaded");
            _progressLogger.Log("Model Loaded");

            return repositoryModel;
        }
        catch (Exception e)
        {
            _logger.Log($"Could not load file at {path} because {e}");
            _progressLogger.Log($"Could not load file at {path} because {e}");
            return null;
        }
    }
}
