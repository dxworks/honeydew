using Honeydew.RepositoryLoading.ProjectRead;
using Honeydew.RepositoryLoading.Strategies;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;

namespace Honeydew;

public class ProjectExtractorFactory
{
    public const string CSharp = "C#";

    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly IProjectLoadingStrategy _projectLoadingStrategy;

    public ProjectExtractorFactory(ILogger logger, IProgressLogger progressLogger,
        IProjectLoadingStrategy projectLoadingStrategy)
    {
        _logger = logger;
        _progressLogger = progressLogger;
        _projectLoadingStrategy = projectLoadingStrategy;
    }

    public ProjectExtractor? GetProjectExtractor(string projectLanguage)
    {
        return projectLanguage switch
        {
            CSharp => new ProjectExtractor(_logger, _progressLogger, _projectLoadingStrategy,
                new CSharpFactExtractor(VisitorLoaderHelper.LoadCSharpVisitors(_logger))),
            _ => null
        };
    }
}
