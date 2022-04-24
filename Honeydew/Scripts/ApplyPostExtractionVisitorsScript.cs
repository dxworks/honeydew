using Honeydew.Logging;
using Honeydew.PostExtraction.ReferenceRelations;
using RepositoryModel = Honeydew.ScriptBeePlugin.Models.RepositoryModel;

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

    public override object RunForResult(Dictionary<string, object?> arguments)
    {
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel")!;

        _logger.Log();
        _logger.Log("Applying Post Extraction Metrics");
        _progressLogger.Log();
        _progressLogger.Log("Applying Post Extraction Metrics");


        IAddStrategy addStrategy = new AddNameStrategy();

        var propertiesRelationVisitor = new PropertiesRelationVisitor(addStrategy);
        var fieldsRelationVisitor = new FieldsRelationVisitor(addStrategy);
        var parameterRelationVisitor = new ParameterRelationVisitor(addStrategy);
        var localVariablesRelationVisitor = new LocalVariablesRelationVisitor(addStrategy);

        var modelVisitors = new List<IEntityModelVisitor>
        {
            propertiesRelationVisitor,
            fieldsRelationVisitor,
            parameterRelationVisitor,
            localVariablesRelationVisitor,

            new ExternCallsRelationVisitor(addStrategy),
            new ExternDataRelationVisitor(addStrategy),
            new HierarchyRelationVisitor(addStrategy),
            new ReturnValueRelationVisitor(addStrategy),
            new DeclarationRelationVisitor(addStrategy, localVariablesRelationVisitor, parameterRelationVisitor,
                fieldsRelationVisitor, propertiesRelationVisitor)
        };

        foreach (var entityModel in repositoryModel.GetEnumerable())
        {
            foreach (var visitor in modelVisitors)
            {
                entityModel[visitor.Name] = visitor.Visit(entityModel);
            }
        }

        return repositoryModel;
    }
}
