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

        [Option("disable-progress-bars", Required = false, Default = false, HelpText = "Disable Progress bars")]
        public bool DisableProgressBars { get; set; }

        [Option("no-bindings", Required = false, Default = false,
            HelpText = "Deactivate Binding Processing")]
        public bool DeactivateBindingProcessing { get; set; }
    }
}
