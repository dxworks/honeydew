using System;
using System.Threading;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewModels;
using HoneydewModels.Importers;

namespace Honeydew.RepositoryLoading;

public class RawFileRepositoryLoader
{
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly JsonModelImporter<RepositoryModel> _jsonModelImporter;

    public RawFileRepositoryLoader(ILogger logger, IProgressLogger progressLogger,
        JsonModelImporter<RepositoryModel> jsonModelImporter)
    {
        _logger = logger;
        _jsonModelImporter = jsonModelImporter;
        _progressLogger = progressLogger;
    }

    public Task<RepositoryModel?> Load(string path, CancellationToken cancellationToken)
    {
        _logger.Log($"Opening File at {path}");
        _progressLogger.Log($"Opening File at {path}");

        try
        {
            var repositoryModel = _jsonModelImporter.Import(path, cancellationToken);

            if (repositoryModel == null)
            {
                return null;
            }

            _logger.Log("Model Loaded");
            _progressLogger.Log("Model Loaded");

            return Task.FromResult(repositoryModel);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not load file at {path} because {e}");
            _progressLogger.Log($"Could not load file at {path} because {e}");
            return null;
        }
    }
}
