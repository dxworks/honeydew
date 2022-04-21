using System.Collections.Generic;
using System.IO;
using HoneydewModels.Exporters;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>statisticsFileOutputName</description>
///     </item>
///     <item>
///         <description>referenceRepositoryModel</description>
///     </item>
/// </list>
/// </summary>
public class ExportStatisticsScript : Script
{
    private readonly JsonModelExporter _repositoryExporter;

    public ExportStatisticsScript(JsonModelExporter repositoryExporter)
    {
        _repositoryExporter = repositoryExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var outputName = VerifyArgument<string>(arguments, "statisticsFileOutputName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

        var fileCount = 0;
        var classCount = 0;
        var delegateCount = 0;
        var interfaceCount = 0;
        var enumCount = 0;
        var sourceCodeLines = 0L;

        foreach (var projectModel in repositoryModel.Projects)
        {
            foreach (var compilationUnit in projectModel.Files)
            {
                fileCount++;
                sourceCodeLines += compilationUnit.Loc.SourceLines;

                foreach (var entityModel in compilationUnit.Entities)
                {
                    switch (entityModel)
                    {
                        case ClassModel:
                            classCount++;
                            break;
                        case DelegateModel:
                            delegateCount++;
                            break;
                        case EnumModel:
                            enumCount++;
                            break;
                        case InterfaceModel:
                            interfaceCount++;
                            break;
                    }
                }
            }
        }

        _repositoryExporter.Export(Path.Combine(outputPath, outputName), new
        {
            repositoryModel.Version,
            Projects = repositoryModel.Projects.Count,
            Solutions = repositoryModel.Solutions.Count,
            Files = fileCount,
            Classes = classCount,
            Delegates = delegateCount,
            Interfaces = interfaceCount,
            Enums = enumCount,
            SourceCodeLines = sourceCodeLines
        });
    }
}
