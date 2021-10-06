using System.Collections.Generic;
using System.IO;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Iterators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Iterators;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

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
    ///         <description>disableSearchForExternTypes</description>
    ///     </item>
    /// </list>
    /// Modifies the following arguments:
    /// <list type="bullet">
    ///     <item>
    ///         <description>repositoryModel</description>
    ///     </item>
    /// </list>
    /// </summary>
    public class ApplyPostExtractionVisitorsScript : Script
    {
        private readonly ILogger _logger;
        private readonly IProgressLogger _progressLogger;

        public ApplyPostExtractionVisitorsScript(ILogger logger, IProgressLogger progressLogger)
        {
            _logger = logger;
            _progressLogger = progressLogger;
        }

        public override void Run(Dictionary<string, object> arguments)
        {
            var outputPath = VerifyArgument<string>(arguments, "outputPath");
            var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "repositoryModel");
            var disableSearchForExternTypes = VerifyArgument<bool>(arguments, "disableSearchForExternTypes");

            _logger.Log();
            _logger.Log("Applying Post Extraction Metrics");
            _progressLogger.Log();
            _progressLogger.Log("Applying Post Extraction Metrics");

            var classNames = new HashSet<string>();

            foreach (var classType in repositoryModel.GetEnumerable())
            {
                classNames.Add(classType.Name);
            }

            var propertiesRelationVisitor = new PropertiesRelationVisitor();
            var fieldsRelationVisitor = new FieldsRelationVisitor();
            var parameterRelationVisitor = new ParameterRelationVisitor();
            var localVariablesRelationVisitor = new LocalVariablesRelationVisitor();

            var modelVisitors = new List<IModelVisitor<IClassType>>
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
            };

            if (!disableSearchForExternTypes)
            {
                var logger = new SerilogLogger(Path.Combine(outputPath, "extern_types.txt"));
                modelVisitors.Add(new ExternEntityTypeVisitor(classNames, logger));
            }

            var repositoryModelIterator = new RepositoryModelIterator(new List<ModelIterator<ProjectModel>>
            {
                new ProjectModelIterator(new List<ModelIterator<ICompilationUnitType>>
                {
                    new CompilationUnitModelIterator(new List<ModelIterator<IClassType>>
                    {
                        new ClassTypePropertyIterator(modelVisitors)
                    })
                })
            });

            repositoryModelIterator.Iterate(repositoryModel);

            arguments["repositoryModel"] = repositoryModel;
        }
    }
}
