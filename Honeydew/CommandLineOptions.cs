using CommandLine;

namespace Honeydew
{
    public class CommandLineOptions
    {
        private const string CommandThatWillBeExecutedMessage = "Command that will be executed";

        [Value(0, Required = true, HelpText = CommandThatWillBeExecutedMessage, Default = "extract")]
        public string Command { get; set; }

        [Value(1, Required = true, HelpText = "Input path that will be analyzed")]
        public string InputFilePath { get; set; }

        [Option(shortName: 'n', longName: "project-name", Required = false,
            HelpText = "Set The Project Name. It is used for Output File Names Generation")]
        public string ProjectName { get; set; }

        [Option("no-progress-bars", Required = false, Default = false, HelpText = "Disable Progress bars")]
        public bool DisableProgressBars { get; set; }

        [Option("no-trim-paths", Required = false, Default = false,
            HelpText = "Deactivate File Path Trimming")]
        public bool DisablePathTrimming { get; set; }

        [Option("voyager", Required = false, Default = false, HelpText = "Use Voyager Options. Export Raw Model")]
        public bool UseVoyager { get; set; }
    }
}
