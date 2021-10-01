using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Logging;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.Core;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Iterators;
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
using HoneydewModels.Types;

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
                IProgressLogger progressLogger =
                    options.DisableProgressBars ? new NoBarsProgressLogger() : new ProgressLogger();

                var honeydewVersion = "";
                try
                {
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    if (version != null)
                    {
                        honeydewVersion = version.ToString();
                    }

                    logger.Log($"Honeydew {honeydewVersion} starting");
                    logger.Log();


                    progressLogger.Log($"Honeydew {version} starting");
                    progressLogger.Log();
                }
                catch (Exception)
                {
                    logger.Log("Could not get Application version", LogLevels.Error);
                    logger.Log();
                }


                logger.Log($"Input Path {options.InputFilePath}");
                logger.Log();

                logger.Log($"Log will be stored at {logFilePath}");
                logger.Log();

                progressLogger.Log($"Log will be stored at {logFilePath}");
                progressLogger.Log();

                if (options.DisableProgressBars)
                {
                    logger.Log("Progress bars are disabled");
                    logger.Log();

                    progressLogger.Log("Progress bars are disabled");
                    progressLogger.Log();
                }

                var inputPath = options.InputFilePath;

                var relationMetricHolder = new RelationMetricHolder();
                RepositoryModel repositoryModel;
                switch (options.Command)
                {
                    case "load":
                    {
                        progressLogger.Log($"Loading model from file {inputPath}");
                        progressLogger.Log();
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

                repositoryModel.Version = honeydewVersion;

                if (!options.DisablePathTrimming)
                {
                    logger.Log();
                    logger.Log("Trimming File Paths");
                    progressLogger.Log();
                    progressLogger.Log("Trimming File Paths");

                    repositoryModel = new FilePathShortenerProcessor(inputPath).Process(repositoryModel);
                }

                if (options.DeactivateBindingProcessing)
                {
                    logger.Log();
                    logger.Log("Applying Post Extraction Metrics");
                    progressLogger.Log();
                    progressLogger.Log("Applying Post Extraction Metrics");

                    ApplyPostExtractionVisitors(repositoryModel);

                    WriteAllRepresentations(repositoryModel,
                        null,
                        DefaultPathForAllRepresentations);
                }
                else
                {
                    logger.Log();
                    logger.Log("Exporting Intermediate Results");
                    progressLogger.Log();
                    progressLogger.Log("Exporting Intermediate Results");

                    WriteRepresentationsToFile(repositoryModel, "_intermediate",
                        DefaultPathForAllRepresentations);

                    logger.Log();
                    logger.Log("Resolving Full Name Dependencies");
                    progressLogger.Log();
                    progressLogger.Log("Resolving Full Name Dependencies");
                    progressLogger.Log();

                    var fqnLogger = new SerilogLogger($"{DefaultPathForAllRepresentations}/fqn_logs.txt");
                    var fullNameModelProcessor = new NewFullNameModelProcessor(logger, fqnLogger, progressLogger,
                        options.DisableLocalVariablesBinding);
                    // var fullNameModelProcessor = new FullNameModelProcessor(logger,progressLogger, options.DisableLocalVariablesBinding);
                    repositoryModel = fullNameModelProcessor.Process(repositoryModel);

                    logger.Log();
                    logger.Log("Applying Post Extraction Metrics");
                    progressLogger.Log();
                    progressLogger.Log("Applying Post Extraction Metrics");

                    ApplyPostExtractionVisitors(repositoryModel);

                    WriteAllRepresentations(repositoryModel,
                        fullNameModelProcessor.NamespacesDictionary,
                        DefaultPathForAllRepresentations);
                }

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

        private static void ApplyPostExtractionVisitors(RepositoryModel repositoryModel)
        {
            var propertiesRelationVisitor = new PropertiesRelationVisitor();
            var fieldsRelationVisitor = new FieldsRelationVisitor();
            var parameterRelationVisitor = new ParameterRelationVisitor();
            var localVariablesRelationVisitor = new LocalVariablesRelationVisitor();

            var repositoryModelIterator = new RepositoryModelIterator(new List<ModelIterator<SolutionModel>>
            {
                new SolutionModelIterator(new List<ModelIterator<ProjectModel>>
                {
                    new ProjectModelIterator(new List<ModelIterator<NamespaceModel>>
                    {
                        new NamespaceModelIterator(new List<ModelIterator<IClassType>>
                        {
                            new ClassTypePropertyIterator(new List<IModelVisitor<IClassType>>
                            {
                                propertiesRelationVisitor,
                                fieldsRelationVisitor,
                                parameterRelationVisitor,
                                localVariablesRelationVisitor,

                                new ExternCallsRelationVisitor(),
                                new HierarchyRelationVisitor(),
                                new ReturnValueRelationVisitor(),
                                new DeclarationRelationVisitor(localVariablesRelationVisitor, parameterRelationVisitor,
                                    fieldsRelationVisitor, propertiesRelationVisitor),
                            })
                        })
                    })
                })
            });

            repositoryModelIterator.Iterate(repositoryModel);
        }

        private static ICompositeVisitor LoadVisitors(IRelationMetricHolder relationMetricHolder, ILogger logger)
        {
            var linesOfCodeVisitor = new LinesOfCodeVisitor();

            var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            });

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });

            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor(),
                attributeSetterVisitor
            });

            var genericParameterSetterVisitor = new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
            {
                new GenericParameterInfoVisitor(),
                attributeSetterVisitor
            });

            var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });

            var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                {
                    calledMethodSetterVisitor,
                    linesOfCodeVisitor,
                    parameterSetterVisitor,
                    localVariablesTypeSetterVisitor,
                    genericParameterSetterVisitor,
                }),
                calledMethodSetterVisitor,
                linesOfCodeVisitor,
                parameterSetterVisitor,
                localVariablesTypeSetterVisitor,
                genericParameterSetterVisitor,
            });

            var methodInfoVisitor = new MethodInfoVisitor();

            var methodVisitors = new List<ICSharpMethodVisitor>
            {
                methodInfoVisitor,
                linesOfCodeVisitor,
                calledMethodSetterVisitor,
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
                parameterSetterVisitor,
                localVariablesTypeSetterVisitor,
                genericParameterSetterVisitor,
            };

            var constructorVisitors = new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                linesOfCodeVisitor,
                calledMethodSetterVisitor,
                new ConstructorCallsVisitor(),
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
                parameterSetterVisitor,
                localVariablesTypeSetterVisitor,
            };

            var fieldVisitors = new List<ICSharpFieldVisitor>
            {
                new FieldInfoVisitor(),
                attributeSetterVisitor,
            };

            var propertyVisitors = new List<ICSharpPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                {
                    methodInfoVisitor,
                    calledMethodSetterVisitor,
                    attributeSetterVisitor,
                    linesOfCodeVisitor,
                    localFunctionsSetterClassVisitor,
                    localVariablesTypeSetterVisitor,
                }),
                linesOfCodeVisitor,
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
                genericParameterSetterVisitor,

                // metrics visitor
                new IsAbstractClassVisitor(),
                new ExceptionsThrownRelationVisitor(relationMetricHolder),
                new ObjectCreationRelationVisitor(relationMetricHolder),
            };

            var delegateVisitors = new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                new ImportsVisitor(),
                attributeSetterVisitor,
                parameterSetterVisitor,
                genericParameterSetterVisitor,
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

            compositeVisitor.Accept(new LoggerSetterVisitor(logger));


            return compositeVisitor;
        }

        private static async Task<RepositoryModel> LoadModel(ILogger logger, string inputPath)
        {
            // Load repository model from path
            IRepositoryLoader<RepositoryModel> repositoryLoader =
                new RawCSharpFileRepositoryLoader(logger, new JsonModelImporter<RepositoryModel>(new ConverterList()));
            var repositoryModel = await repositoryLoader.Load(inputPath);
            return repositoryModel;
        }

        private static async Task<RepositoryModel> ExtractModel(ILogger logger, IProgressLogger progressLogger,
            IRelationMetricHolder relationMetricHolder,
            string inputPath)
        {
            var solutionProvider = new MsBuildSolutionProvider();
            var projectProvider = new MsBuildProjectProvider();
            ICompilationMaker compilationMaker = new CSharpCompilationMaker();
            // Create repository model from path
            var projectLoadingStrategy = new BasicProjectLoadingStrategy(logger, compilationMaker);

            var solutionLoadingStrategy =
                new BasicSolutionLoadingStrategy(logger, projectLoadingStrategy, progressLogger);

            var repositoryLoader = new CSharpRepositoryLoader(solutionProvider, projectProvider, projectLoadingStrategy,
                solutionLoadingStrategy, logger, progressLogger,
                new FactExtractorCreator(LoadVisitors(relationMetricHolder, logger), compilationMaker));
            var repositoryModel = await repositoryLoader.Load(inputPath);

            return repositoryModel;
        }

        private static void WriteAllRepresentations(RepositoryModel repositoryModel,
            IDictionary<string, NamespaceTree> fullNameNamespaces, string outputPath)
        {
            WriteRepresentationsToFile(repositoryModel, "", outputPath);

            if (fullNameNamespaces != null)
            {
                var fullNameNamespacesExporter = new JsonModelExporter();
                fullNameNamespacesExporter.Export(Path.Combine(outputPath, "honeydew_namespaces.json"),
                    fullNameNamespaces);
            }
        }

        private static void WriteRepresentationsToFile(RepositoryModel repositoryModel, string nameModifier,
            string outputPath)
        {
            var repositoryExporter = new JsonModelExporter();
            repositoryExporter.Export(Path.Combine(outputPath, $"honeydew{nameModifier}.json"), repositoryModel);

            var csvModelExporter = GetRelationsRepresentationExporter();

            var classRelationsRepresentation = GetClassRelationsRepresentation(repositoryModel);
            csvModelExporter.Export(Path.Combine(outputPath, $"honeydew{nameModifier}.csv"),
                classRelationsRepresentation);

            //var allFileRelationsRepresentation =
            //     new RepositoryModelToFileRelationsProcessor(new ChooseAllStrategy()).Process(repositoryModel);
            // csvModelExporter.Export(Path.Combine(outputPath, $"honeydew_file_relations_all{nameModifier}.csv"),
            //    allFileRelationsRepresentation);

            var jafaxFileRelationsRepresentation =
                 new RepositoryModelToFileRelationsProcessor(new JafaxChooseStrategy()).Process(repositoryModel);
             csvModelExporter.Export(Path.Combine(outputPath, $"honeydew_file_relations{nameModifier}.csv"),
                 jafaxFileRelationsRepresentation, new List<string>
                 {
                     "extCalls",
                     "hierarchy",
                     "returns",
                     "declarations"
                 });

            var cyclomaticComplexityPerFileRepresentation =
                GetCyclomaticComplexityPerFileRepresentation(repositoryModel);
            var cyclomaticComplexityPerFileExporter = new JsonModelExporter();
            cyclomaticComplexityPerFileExporter.Export(
                Path.Combine(outputPath, $"honeydew_cyclomatic{nameModifier}.json"),
                cyclomaticComplexityPerFileRepresentation);
        }

        private static CsvRelationsRepresentationExporter GetRelationsRepresentationExporter()
        {
            var csvModelExporter = new CsvRelationsRepresentationExporter
            {
                ColumnFunctionForEachRow = new List<Tuple<string, Func<string, string>>>
                {
                    new("all", ExportUtils.CsvSumPerLine)
                }
            };

            return csvModelExporter;
        }

        private static RelationsRepresentation GetClassRelationsRepresentation(
            RepositoryModel repositoryModel)
        {
            var classRelationsRepresentation =
                new RepositoryModelToClassRelationsProcessor()
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
