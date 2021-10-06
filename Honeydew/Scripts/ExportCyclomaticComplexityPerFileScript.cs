﻿using System.Collections.Generic;
using System.IO;
using HoneydewCore.Processors;
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
    ///         <description>cycloOutputName</description>
    ///     </item>
    ///     <item>
    ///         <description>repositoryModel</description>
    ///     </item>
    /// </list>
    /// </summary>
    public class ExportCyclomaticComplexityPerFileScript : Script
    {
        private readonly JsonModelExporter _repositoryExporter;

        public ExportCyclomaticComplexityPerFileScript(JsonModelExporter repositoryExporter)
        {
            _repositoryExporter = repositoryExporter;
        }

        public override void Run(Dictionary<string, object> arguments)
        {
            var outputPath = VerifyArgument<string>(arguments, "outputPath");
            var outputName = VerifyArgument<string>(arguments, "cycloOutputName");
            var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "repositoryModel");

            var cyclomaticComplexityPerFileRepresentation =
                new RepositoryModelToCyclomaticComplexityPerFileProcessor().Process(repositoryModel);

            _repositoryExporter.Export(Path.Combine(outputPath, outputName), cyclomaticComplexityPerFileRepresentation);
        }
    }
}
