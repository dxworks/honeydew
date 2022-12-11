using Honeydew.DesignSmellsDetection.Runner;
using Honeydew.Extractors.Exporters;
using Honeydew.Logging;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.Scripts;

public class ExportDesignSmellsPerFileScript : Script
{
    private readonly JsonModelExporter _modelExporter;
    private readonly ILogger _logger;

    public ExportDesignSmellsPerFileScript(JsonModelExporter modelExporter, ILogger logger)
    {
        _modelExporter = modelExporter;
        _logger = logger;
    }

    public override void Run(Dictionary<string, object?> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath")!;
        var outputName = VerifyArgument<string>(arguments, "designSmellsOutputName")!;
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel")!;

        var designSmells = new DesignSmellsDetectionRunner(_logger).Detect(repositoryModel).OrderBy(ds => ds.SourceFile);

        DesignSmellsJsonWriter.Export(_modelExporter, designSmells, Path.Combine(outputPath, outputName));
    }
}
