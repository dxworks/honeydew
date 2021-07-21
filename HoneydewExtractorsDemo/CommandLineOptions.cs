using CommandLine;

namespace HoneydewExtractorsDemo
{
    public class CommandLineOptions
    {
        private const string CommandThatWillBeExecutedMessage = @"Command that will be executed";

        [Value(0, Required = true, HelpText = CommandThatWillBeExecutedMessage, Default = "extract")]
        public string Command { get; set; }

        [Value(1, Required = true, HelpText = "Input path that will be analyzed")]
        public string InputFilePath { get; set; }
    }
}
