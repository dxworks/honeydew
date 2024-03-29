﻿using CommandLine;

namespace Honeydew;

public class CommandLineOptions
{
    [Value(0, Required = true, HelpText = "Input path that will be analyzed")]
    public string InputFilePath { get; set; } = "";

    [Option('n', "project-name", Required = false,
        HelpText = "Set The Project Name. It is used for Output File Names Generation")]
    public string ProjectName { get; set; } = "";

    [Option("no-progress-bars", Required = false, Default = false, HelpText = "Disable Progress bars")]
    public bool DisableProgressBars { get; set; } = false;
}

[Verb("extract")]
public class ExtractOptions : CommandLineOptions
{
    [Option("no-trim-paths", Required = false, Default = false, HelpText = "Deactivate File Path Trimming")]
    public bool DisablePathTrimming { get; set; } = false;

    [Option('p', "parallel", Required = false, Default = false, HelpText = "Parallel Extraction")]
    public bool ParallelExtraction { get; set; } = false;
}

[Verb("load")]
public class LoadOptions : CommandLineOptions
{
    [Option('p', "parallel", Required = false, Default = false, HelpText = "Parallel Script Running")]
    public bool ParallelRunning { get; set; } = false;
}

[Verb("adapt")]
public class AdaptOptions : CommandLineOptions
{
}
