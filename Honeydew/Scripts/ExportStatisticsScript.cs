using System.Collections.Generic;
using System.IO;
using HoneydewModels.CSharp;
using HoneydewModels.Exporters;

namespace Honeydew.Scripts
{
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
    ///         <description>repositoryModel</description>
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
            var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "repositoryModel");

            var fileCount = 0;
            var classCount = 0;
            var delegateCount = 0;
            var sourceCodeLines = 0L;

            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var compilationUnit in projectModel.CompilationUnits)
                {
                    fileCount++;
                    sourceCodeLines += compilationUnit.Loc.SourceLines;

                    foreach (var classType in compilationUnit.ClassTypes)
                    {
                        if (classType is DelegateModel)
                        {
                            delegateCount++;
                        }
                        else
                        {
                            classCount++;
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
                SourceCodeLines = sourceCodeLines
            });
        }
    }
}
