using System.Collections.Generic;
using System.IO;
using Honeydew.IO.Writers.Exporters;
using Honeydew.Processors;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.Scripts
{
    /// <summary>
    /// Requires the following arguments:
    /// <list type="bullet">
    ///     <item>
    ///         <description>outputPath</description>
    ///     </item>
    ///     <item>
    ///         <description>classRelationsOutputName</description>
    ///     </item>
    ///     <item>
    ///         <description>referenceRepositoryModel</description>
    ///     </item>
    /// </list>
    /// </summary>
    public class ExportClassRelationsScript : Script
    {
        private readonly CsvRelationsRepresentationExporter _representationExporter;

        public ExportClassRelationsScript(CsvRelationsRepresentationExporter representationExporter)
        {
            _representationExporter = representationExporter;
        }

        public override void Run(Dictionary<string, object> arguments)
        {
            var outputPath = VerifyArgument<string>(arguments, "outputPath");
            var outputName = VerifyArgument<string>(arguments, "classRelationsOutputName");
            var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

            var classRelationsRepresentation =
                new RepositoryModelToClassRelationsProcessor(new HoneydewChooseStrategy()).Process(repositoryModel);

            _representationExporter.Export(Path.Combine(outputPath, outputName), classRelationsRepresentation);
        }
    }
}
