using System.Collections.Generic;
using System.IO;
using Honeydew.Models;
using Honeydew.Models.Exporters;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>rawJsonOutputName</description>
///     </item>
///     <item>
///         <description>repositoryModel</description>
///     </item>
/// </list>
/// </summary>
public class ExportRawModelScript : Script
{
    private readonly JsonModelExporter _repositoryExporter;
        
    public ExportRawModelScript(JsonModelExporter repositoryExporter)
    {
        _repositoryExporter = repositoryExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var outputName = VerifyArgument<string>(arguments, "rawJsonOutputName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "repositoryModel");

        _repositoryExporter.Export(Path.Combine(outputPath, outputName), repositoryModel);
    }
}
