using System;
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
using HoneydewExtractors.Core.Metrics.Extraction.Attribute;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Constructor;
using HoneydewExtractors.Core.Metrics.Extraction.Delegate;
using HoneydewExtractors.Core.Metrics.Extraction.Field;
using HoneydewExtractors.Core.Metrics.Extraction.Method;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Property;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewExtractors.CSharp.RepositoryLoading;
using HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewExtractors.Processors;
using HoneydewModels;
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
                var progressLogger = new ProgressLogger();

                logger.Log($"Log will be stored at {logFilePath}");
                logger.Log();

                progressLogger.Log($"Log will be stored at {logFilePath}");
                progressLogger.Log();

                var inputPath = options.InputFilePath;

                var relationMetricHolder = new RelationMetricHolder();
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
                        repositoryModel = await ExtractModel(logger, progressLogger, relationMetricHolder, inputPath);
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
                progressLogger.Log();
                progressLogger.Log("Trimming File Paths");

                repositoryModel = new FilePathShortenerProcessor(inputPath).Process(repositoryModel);

                logger.Log();
                logger.Log("Exporting Intermediate Results");
                progressLogger.Log();
                progressLogger.Log("Exporting Intermediate Results");

                WriteRepresentationsToFile(repositoryModel, relationMetricHolder, "_intermediate",
                    DefaultPathForAllRepresentations);


                logger.Log();
                logger.Log("Resolving Full Name Dependencies");
                progressLogger.Log();
                progressLogger.Log("Resolving Full Name Dependencies");
                progressLogger.Log();

                // Post Extraction Repository model processing
                var fullNameModelProcessor = new FullNameModelProcessor(logger, progressLogger);
                repositoryModel = fullNameModelProcessor.Process(repositoryModel);


                WriteAllRepresentations(repositoryModel, relationMetricHolder,
                    fullNameModelProcessor.NamespacesDictionary,
                    DefaultPathForAllRepresentations);

                logger.Log();
                logger.Log("Extraction Complete!");
                logger.Log();
                logger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

                progressLogger.Log();
                progressLogger.Log("Extraction Complete!");
                progressLogger.Log();
                progressLogger.Log($"Output will be found at {Path.GetFullPath(DefaultPathForAllRepresentations)}");

                logger.CloseAndFlush();
            }, _ => Task.FromResult("Some Error Occurred"));
        }

        private static ICompositeVisitor LoadVisitors(IRelationMetricHolder relationMetricHolder)
        {
            var linesOfCodeVisitor = new LinesOfCodeVisitor();

            var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            });

            var calledMethodSetterVisitor =
                new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
                {
                    new MethodCallInfoVisitor()
                });

            var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                {
                    calledMethodSetterVisitor,
                    linesOfCodeVisitor,
                }),
                calledMethodSetterVisitor,
                linesOfCodeVisitor,
            });

            var methodVisitors = new List<ICSharpMethodVisitor>
            {
                new MethodInfoVisitor(),
                linesOfCodeVisitor,
                calledMethodSetterVisitor,
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
            };

            var constructorVisitors = new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                linesOfCodeVisitor,
                calledMethodSetterVisitor,
                new ConstructorCallsVisitor(),
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
            };

            var fieldVisitors = new List<ICSharpFieldVisitor>
            {
                new FieldInfoVisitor(),
                attributeSetterVisitor,
            };

            var propertyVisitors = new List<ICSharpPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                calledMethodSetterVisitor,
                linesOfCodeVisitor,
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
            };

            var classVisitors = new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new MethodSetterClassVisitor(methodVisitors),
                new ConstructorSetterClassVisitor(constructorVisitors),
                new FieldSetterClassVisitor(fieldVisitors),
                new PropertySetterClassVisitor(propertyVisitors),
                new ImportsVisitor(),
                linesOfCodeVisitor,
                attributeSetterVisitor,

                // metrics visitor
                new IsAbstractClassVisitor(),
                new FieldsRelationVisitor(relationMetricHolder),
                new PropertiesRelationVisitor(relationMetricHolder),
                new ParameterRelationVisitor(relationMetricHolder),
                new ReturnValueRelationVisitor(relationMetricHolder),
                new ExceptionsThrownRelationVisitor(relationMetricHolder),
                new ObjectCreationRelationVisitor(relationMetricHolder),
                new LocalVariablesRelationVisitor(relationMetricHolder)
            };

            var delegateVisitors = new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                new ImportsVisitor(),
                attributeSetterVisitor,
            };
            var compilationUnitVisitors = new List<ICSharpCompilationUnitVisitor>
            {
                new ClassSetterCompilationUnitVisitor(classVisitors),
                new DelegateSetterCompilationUnitVisitor(delegateVisitors),
                new ImportsVisitor(),
                linesOfCodeVisitor,
            };

            var compositeVisitor = new CompositeVisitor();

            foreach (var compilationUnitVisitor in compilationUnitVisitors)
            {
                compositeVisitor.Add(compilationUnitVisitor);
            }

            return compositeVisitor;
        }

        private static async Task<RepositoryModel> LoadModel(ILogger logger, string inputPath)
        {
            // Load repository model from path
            IRepositoryLoader<RepositoryModel> repositoryLoader =
                new RawCSharpFileRepositoryLoader(logger, new FileReader(), new JsonRepositoryModelImporter());
            var repositoryModel = await repositoryLoader.Load(inputPath);
            return repositoryModel;
        }

        private static async Task<RepositoryModel> ExtractModel(ILogger logger, IProgressLogger progressLogger,
            IRelationMetricHolder relationMetricHolder,
            string inputPath)
        {
            var solutionProvider = new MsBuildSolutionProvider();
            var projectProvider = new MsBuildProjectProvider();

            // Create repository model from path
            var projectLoadingStrategy = new BasicProjectLoadingStrategy(logger);

            var solutionLoadingStrategy =
                new BasicSolutionLoadingStrategy(logger, projectLoadingStrategy, progressLogger);

            var repositoryLoader = new CSharpRepositoryLoader(solutionProvider, projectProvider, projectLoadingStrategy,
                solutionLoadingStrategy, logger, progressLogger,
                new FactExtractorCreator(LoadVisitors(relationMetricHolder)));
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }

        private static void WriteAllRepresentations(RepositoryModel repositoryModel,
            IRelationMetricHolder relationMetricHolder,
            IDictionary<string, NamespaceTree> fullNameNamespaces, string outputPath)
        {
            var writer = new FileWriter();

            WriteRepresentationsToFile(repositoryModel, relationMetricHolder, "", outputPath);

            var fullNameNamespacesExporter = new JsonFullNameNamespaceDictionaryExporter();
            writer.WriteFile(Path.Combine(outputPath, "honeydew_namespaces.json"),
                fullNameNamespacesExporter.Export(fullNameNamespaces));
        }

        private static void WriteRepresentationsToFile(RepositoryModel repositoryModel,
            IRelationMetricHolder relationMetricHolder, string nameModifier,
            string outputPath)
        {
            var writer = new FileWriter();

            var repositoryExporter = GetRepositoryModelExporter();
            writer.WriteFile(Path.Combine(outputPath, $"honeydew{nameModifier}.json"),
                repositoryExporter.Export(repositoryModel));

            var classRelationsRepresentation = GetClassRelationsRepresentation(relationMetricHolder);
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
            return new JsonRepositoryModelExporter(new ConverterList());
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

        private static ClassRelationsRepresentation GetClassRelationsRepresentation(
            IRelationMetricHolder relationMetricHolder)
        {
            var classRelationsRepresentation =
                new RelationMetricHolderToClassRelationsProcessor()
                    .Process(relationMetricHolder);
            return classRelationsRepresentation;
        }

        private static CyclomaticComplexityPerFileRepresentation GetCyclomaticComplexityPerFileRepresentation(
            RepositoryModel repositoryModel)
        {
            return new RepositoryModelToCyclomaticComplexityPerFileProcessor().Process(repositoryModel);
        }
    }
}
