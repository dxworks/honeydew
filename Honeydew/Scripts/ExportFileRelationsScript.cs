using System.Collections.Generic;
using System.IO;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewExtractors.Processors;
using HoneydewModels.Reference;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>fileRelationsOutputName</description>
///     </item>
///     <item>
///         <description>referenceRepositoryModel</description>
///     </item>
///     <item>
///         <description>fileRelationsStrategy</description>
///     </item>
///     <item>
///         <description>fileRelationsHeaders</description>
///     </item>
/// </list>
/// </summary>
public class ExportFileRelationsScript : Script
{
    private readonly CsvRelationsRepresentationExporter _representationExporter;

    public ExportFileRelationsScript(CsvRelationsRepresentationExporter representationExporter)
    {
        _representationExporter = representationExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var outputName = VerifyArgument<string>(arguments, "fileRelationsOutputName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");
        var strategy = VerifyArgument<IRelationsMetricChooseStrategy>(arguments, "fileRelationsStrategy");
        var csvHeaders = VerifyArgument<List<string>>(arguments, "fileRelationsHeaders");

        var allFileRelationsRepresentation =
            new RepositoryModelToFileRelationsProcessor(strategy).Process(repositoryModel);

        _representationExporter.Export(Path.Combine(outputPath, outputName), allFileRelationsRepresentation,
            csvHeaders);
    }
}
