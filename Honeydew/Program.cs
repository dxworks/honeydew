﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Writers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Logging;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.Core;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using HoneydewExtractors.CSharp.RepositoryLoading;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;
using HoneydewModels.Exporters;
using HoneydewModels.Importers;

namespace Honeydew
{
    class Program
    {
        private const string DefaultPathForAllRepresentations = "results";

        public static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            await result.MapResult(async options =>
            {
                var inputPath = options.InputFilePath;
                var progressLogger = new ConsoleProgressLogger();

                RepositoryModel repositoryModel;
                switch (options.Command)
                {
                    case "load":
                    {
                        repositoryModel = await LoadModel(progressLogger, inputPath);
                    }
                        break;

                    case "extract":
                    {
                        repositoryModel = await ExtractModel(progressLogger, inputPath);
                    }
                        break;

                    default:
                    {
                        await Console.Error.WriteLineAsync("Invalid Command! Please use extract or load");
                        return;
                    }
                }

                progressLogger.LogLine("Resolving Full Name Dependencies");

                ConsoleLoggerWithHistory consoleLoggerWithHistory = new(new ConsoleProgressLogger());

                // Post Extraction Repository model processing
                var fullNameModelProcessor = new FullNameModelProcessor(consoleLoggerWithHistory);
                repositoryModel = fullNameModelProcessor.Process(repositoryModel);

                repositoryModel = new FilePathShortenerProcessor(inputPath).Process(repositoryModel);


                WriteAllRepresentations(repositoryModel, fullNameModelProcessor.NamespacesDictionary,
                    consoleLoggerWithHistory, DefaultPathForAllRepresentations);

                progressLogger.LogLine("Extraction Complete!");
                progressLogger.LogLine($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");
            }, _ => Task.FromResult("Some Error Occurred"));
        }

        private static CSharpFactExtractor LoadExtractor()
        {
            var cSharpFactExtractor = new CSharpFactExtractor();
            cSharpFactExtractor.AddMetric<CSharpUsingsCountMetric>();

            cSharpFactExtractor.AddMetric<CSharpPropertiesRelationMetric>();
            cSharpFactExtractor.AddMetric<CSharpFieldsRelationMetric>();
            cSharpFactExtractor.AddMetric<CSharpParameterRelationMetric>();
            cSharpFactExtractor.AddMetric<CSharpReturnValueRelationMetric>();
            cSharpFactExtractor.AddMetric<CSharpLocalVariablesRelationMetric>();
            cSharpFactExtractor.AddMetric<CSharpObjectCreationRelationMetric>();
            cSharpFactExtractor.AddMetric<CSharpExceptionsThrownRelationMetric>();

            return cSharpFactExtractor;
        }

        private static async Task<RepositoryModel> LoadModel(IProgressLogger progressLogger, string inputPath)
        {
            // Load repository model from path
            IRepositoryLoader<RepositoryModel> repositoryLoader =
                new RawCSharpFileRepositoryLoader(progressLogger, new FileReader(), new JsonRepositoryModelImporter());
            var repositoryModel = await repositoryLoader.Load(inputPath);
            return repositoryModel;
        }

        private static async Task<RepositoryModel> ExtractModel(IProgressLogger progressLogger, string inputPath)
        {
            // Create repository model from path
            var projectLoadingStrategy = new BasicProjectLoadingStrategy(progressLogger);
            var solutionLoadingStrategy = new BasicSolutionLoadingStrategy(progressLogger, projectLoadingStrategy);

            var repositoryLoader = new CSharpRepositoryLoader(projectLoadingStrategy, solutionLoadingStrategy,
                progressLogger, LoadExtractor());
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }

        private static void WriteAllRepresentations(RepositoryModel repositoryModel,
            IDictionary<string, NamespaceTree> fullNameNamespaces,
            ConsoleLoggerWithHistory consoleLoggerWithHistory, string outputPath)
        {
            var writer = new FileWriter();

            var repositoryExporter = GetRepositoryModelExporter();
            writer.WriteFile(Path.Combine(outputPath, "honeydew.json"), repositoryExporter.Export(repositoryModel));

            var classRelationsRepresentation = GetClassRelationsRepresentation(repositoryModel);
            var csvModelExporter = GetClassRelationsRepresentationExporter();
            writer.WriteFile(Path.Combine(outputPath, "honeydew.csv"),
                csvModelExporter.Export(classRelationsRepresentation));

            var fullNameNamespacesExporter = new JsonFullNameNamespaceDictionaryExporter();
            writer.WriteFile(Path.Combine(outputPath,"honeydew_namespaces.json"), fullNameNamespacesExporter.Export(fullNameNamespaces));

            var ambiguousHistory = consoleLoggerWithHistory.GetHistory();
            if (!string.IsNullOrEmpty(ambiguousHistory))
            {
                writer.WriteFile(Path.Combine(outputPath, "honeydew_ambiguous.txt"), ambiguousHistory);
            }
        }

        private static IModelExporter<RepositoryModel> GetRepositoryModelExporter()
        {
            return new JsonRepositoryModelExporter();
        }

        private static IModelExporter<ClassRelationsRepresentation> GetClassRelationsRepresentationExporter()
        {
            var csvModelExporter = new CsvClassRelationsRepresentationExporter
            {
                ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                {
                    new("Total Count", ExportUtils.CsvSumPerLine)
                }
            };

            return csvModelExporter;
        }

        private static ClassRelationsRepresentation GetClassRelationsRepresentation(RepositoryModel repositoryModel)
        {
            var classRelationsRepresentation =
                new RepositoryModelToClassRelationsProcessor(new MetricRelationsProvider(), new MetricPrettier(), true)
                    .Process(repositoryModel);
            return classRelationsRepresentation;
        }
    }
}
