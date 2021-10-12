using System.Collections.Generic;
using System.IO;
using HoneydewCore.Logging;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;

namespace Honeydew.Scripts
{
    /// <summary>
    /// Requires the following arguments:
    /// <list type="bullet">
    ///     <item>
    ///         <description>outputPath</description>
    ///     </item>
    ///     <item>
    ///         <description>repositoryModel</description>
    ///     </item>
    ///     <item>
    ///         <description>disableLocalVariablesBinding</description>
    ///     </item>
    /// </list>
    /// Modifies the following arguments:
    /// <list type="bullet">
    ///     <item>
    ///         <description>repositoryModel</description>
    ///     </item>
    /// </list>
    /// </summary>
    public class FullNameModelProcessorScript : Script
    {
        private readonly ILogger _logger;
        private readonly IProgressLogger _progressLogger;

        public FullNameModelProcessorScript(ILogger logger, IProgressLogger progressLogger)
        {
            _logger = logger;
            _progressLogger = progressLogger;
        }

        public override void Run(Dictionary<string, object> arguments)
        {
            var outputPath = VerifyArgument<string>(arguments, "outputPath");
            var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "repositoryModel");
            var disableLocalVariablesBinding = VerifyArgument<bool>(arguments, "disableLocalVariablesBinding");

            var fqnLogger = new SerilogLogger(Path.Combine(outputPath, "fqn_logs.txt"));
            var fullNameModelProcessor = new FullNameModelProcessor(_logger, fqnLogger, _progressLogger, disableLocalVariablesBinding);

            repositoryModel = fullNameModelProcessor.Process(repositoryModel);

            arguments["repositoryModel"] = repositoryModel;
        }
    }
}
