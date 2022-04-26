using Honeydew.Extraction;
using Honeydew.Extractors.CSharp;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Dotnet.Load;
using Honeydew.Extractors.Dotnet.Load.Strategies;
using Honeydew.Extractors.Load;
using Honeydew.Logging;

namespace Honeydew;

public class ProjectExtractorFactory
{
    public const string CSharp = "C#";
    public const string VisualBasic = "VisualBasic";

    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;
    private readonly bool _parallelExtraction;

    public ProjectExtractorFactory(ILogger logger, IProgressLogger progressLogger, bool parallelExtraction)
    {
        _logger = logger;
        _progressLogger = progressLogger;
        _parallelExtraction = parallelExtraction;
    }

    public DotnetProjectExtractor? GetProjectExtractor(string projectLanguage)
    {
        return projectLanguage switch
        {
            CSharp => new DotnetProjectExtractor(_logger, _progressLogger,
                GetProjectLoadingStrategy(_logger, _parallelExtraction),
                new CSharpFactExtractor(CSharpExtractionVisitors.GetVisitors(_logger)), new MsBuildProjectProvider()),
            // VisualBasic => new ProjectExtractor(_logger, _progressLogger, _projectLoadingStrategy,
            //     new VisualBasicFactExtractor(VisualBasicExtractionVisitors.GetVisitors(_logger)), new MsBuildProjectProvider()),
            _ => null
        };
    }

    private static IDotnetProjectLoadingStrategy GetProjectLoadingStrategy(ILogger logger, bool parallelExtraction)
    {
        return parallelExtraction
            ? new ParallelDotnetProjectLoadingStrategy(logger, new ActualFilePathProvider(logger))
            : new BasicDotnetProjectLoadingStrategy(logger, new ActualFilePathProvider(logger));
    }
}
