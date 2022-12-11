using DxWorks.ScriptBee.Plugins.Honeydew;
using Honeydew.Logging;
using Honeydew.Models;
using DxWorks.ScriptBee.Plugins.Honeydew;

namespace Honeydew;

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

    public async Task<RepositoryModel?> LoadAsync(string path, CancellationToken cancellationToken)
    {
        _logger.Log($"Opening File at {path}");
        _progressLogger.Log($"Opening File at {path}");

        await using var file = File.OpenRead(path);

        try
        {
            return await LoadAsync(file, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Log($"Could not load file at {path} because {e}");
            _progressLogger.Log($"Could not load file at {path} because {e}");
            return null;
        }
    }

    public async Task<RepositoryModel?> LoadAsync(Stream inputStream, CancellationToken cancellationToken)
    {
        var repositoryModel = await _repositoryModelImporter.Import(inputStream, cancellationToken);

        if (repositoryModel == null)
        {
            return null;
        }

        _logger.Log("Model Loaded");
        _progressLogger.Log("Model Loaded");

        return repositoryModel;
    }
}
