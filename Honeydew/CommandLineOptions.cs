﻿using CommandLine;

namespace Honeydew
{
    public class CommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Path to solution that will be analyzed")]
        public string InputFilePath { get; set; }

        [Value(1, Required = false, Default = "",
            HelpText = "Output Path of The Model. If not present, then result will pe printed to the standard output")]
        public string OutputFilePath { get; set; }

        [Option('r', "representation", Required = false, HelpText = "The Representation of the output model",
            Default = "normal")]
        public string RepresentationType { get; set; }

        [Option('e', "export-type", Required = false, HelpText = "The export type of the model",
            Default = "json")]
        public string ExportType { get; set; }
    }
}