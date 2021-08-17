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
                var logFilePath = $"{DefaultPathForAllRepresentations}/logs.txt";
                var logger = new SerilogLogger(logFilePath);

                logger.Log($"Log will be stored at {logFilePath}");
                logger.Log();

                var inputPath = options.InputFilePath;
                var repositoryClassSet = new RepositoryClassSet();

                RepositoryModel repositoryModel;
                switch (options.Command)
                {
                    case "load":
                    {
                        repositoryModel = await LoadModel(logger, inputPath);
                    }
                        break;

                    case "extract":
                    {
                        repositoryModel = await ExtractModel(logger, repositoryClassSet, inputPath);
                    }
                        break;

                    default:
                    {
                        await Console.Error.WriteLineAsync("Invalid Command! Please use extract or load");
                        return;
                    }
                }

                logger.Log();
                logger.Log("Trimming File Paths");

                repositoryModel = new FilePathShortenerProcessor(inputPath).Process(repositoryModel);

                logger.Log();
                logger.Log("Exporting Intermediate Results");

                WriteRepresentationsToFile(repositoryModel, "_intermediate", DefaultPathForAllRepresentations);


                logger.Log();
                logger.Log("Resolving Full Name Dependencies");

                // Post Extraction Repository model processing
                var fullNameModelProcessor = new FullNameModelProcessor(logger, repositoryClassSet);
                repositoryModel = fullNameModelProcessor.Process(repositoryModel);


                WriteAllRepresentations(repositoryModel, fullNameModelProcessor.NamespacesDictionary,
                    DefaultPathForAllRepresentations);

                logger.Log();
                logger.Log("Extraction Complete!");
                logger.Log();
                logger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

                logger.CloseAndFlush();
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

        private static async Task<RepositoryModel> LoadModel(ILogger logger, string inputPath)
        {
            // Load repository model from path
            IRepositoryLoader<RepositoryModel> repositoryLoader =
                new RawCSharpFileRepositoryLoader(logger, new FileReader(), new JsonRepositoryModelImporter());
            var repositoryModel = await repositoryLoader.Load(inputPath);
            return repositoryModel;
        }

        private static async Task<RepositoryModel> ExtractModel(ILogger logger, RepositoryClassSet repositoryClassSet,
            string inputPath)
        {
            // Create repository model from path
            var projectLoadingStrategy = new BasicProjectLoadingStrategy(logger, repositoryClassSet);
            var solutionLoadingStrategy = new BasicSolutionLoadingStrategy(logger, projectLoadingStrategy);

            var repositoryLoader = new CSharpRepositoryLoader(projectLoadingStrategy, solutionLoadingStrategy,
                logger, LoadExtractor());
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }

        private static void WriteAllRepresentations(RepositoryModel repositoryModel,
            IDictionary<string, NamespaceTree> fullNameNamespaces, string outputPath)
        {
            var writer = new FileWriter();

            WriteRepresentationsToFile(repositoryModel, "", outputPath);

            var fullNameNamespacesExporter = new JsonFullNameNamespaceDictionaryExporter();
            writer.WriteFile(Path.Combine(outputPath, "honeydew_namespaces.json"),
                fullNameNamespacesExporter.Export(fullNameNamespaces));
        }

        private static void WriteRepresentationsToFile(RepositoryModel repositoryModel, string nameModifier,
            string outputPath)
        {
            var writer = new FileWriter();

            var repositoryExporter = GetRepositoryModelExporter();
            writer.WriteFile(Path.Combine(outputPath, $"honeydew{nameModifier}.json"),
                repositoryExporter.Export(repositoryModel));

            var classRelationsRepresentation = GetClassRelationsRepresentation(repositoryModel);
            var csvModelExporter = GetClassRelationsRepresentationExporter();
            writer.WriteFile(Path.Combine(outputPath, $"honeydew{nameModifier}.csv"),
                csvModelExporter.Export(classRelationsRepresentation));

            var cyclomaticComplexityPerFileRepresentation =
                GetCyclomaticComplexityPerFileRepresentation(repositoryModel);
            var cyclomaticComplexityPerFileExporter = GetCyclomaticComplexityPerFileExporter();
            writer.WriteFile(Path.Combine(outputPath, $"honeydew_cyclomatic{nameModifier}.json"),
                cyclomaticComplexityPerFileExporter.Export(cyclomaticComplexityPerFileRepresentation));
        }

        private static IModelExporter<CyclomaticComplexityPerFileRepresentation>
            GetCyclomaticComplexityPerFileExporter()
        {
            return new JsonCyclomaticComplexityPerFileRepresentationExporter();
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

        private static CyclomaticComplexityPerFileRepresentation GetCyclomaticComplexityPerFileRepresentation(
            RepositoryModel repositoryModel)
        {
            return new RepositoryModelToCyclomaticComplexityPerFileProcessor().Process(repositoryModel);
        }
    }
}
