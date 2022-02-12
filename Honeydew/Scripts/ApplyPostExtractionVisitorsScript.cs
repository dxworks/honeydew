using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewModels.Reference;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>repositoryModel</description>
///     </item>
/// </list>
/// Returns:
/// <list type="bullet">
///     <item>
///         <description>referenceRepositoryModel</description>
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

    public override object RunForResult(Dictionary<string, object> arguments)
    {
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

        _logger.Log();
        _logger.Log("Applying Post Extraction Metrics");
        _progressLogger.Log();
        _progressLogger.Log("Applying Post Extraction Metrics");


        var propertiesRelationVisitor = new PropertiesRelationVisitor();
        var fieldsRelationVisitor = new FieldsRelationVisitor();
        var parameterRelationVisitor = new ParameterRelationVisitor();
        var localVariablesRelationVisitor = new LocalVariablesRelationVisitor();

        var modelVisitors = new List<IModelVisitor<ClassModel>>
        {
            propertiesRelationVisitor,
            fieldsRelationVisitor,
            parameterRelationVisitor,
            localVariablesRelationVisitor,

            new ExternCallsRelationVisitor(),
            new ExternDataRelationVisitor(),
            new HierarchyRelationVisitor(),
            new ReturnValueRelationVisitor(),
            new DeclarationRelationVisitor(localVariablesRelationVisitor, parameterRelationVisitor,
                fieldsRelationVisitor, propertiesRelationVisitor),
        };

        foreach (var classOption in repositoryModel.GetEnumerable())
        {
            if (classOption is not ClassOption.Class(var classModel)) continue;

            foreach (var visitor in modelVisitors)
            {
                visitor.Visit(classModel);
            }
        }

        return repositoryModel;
    }
}
