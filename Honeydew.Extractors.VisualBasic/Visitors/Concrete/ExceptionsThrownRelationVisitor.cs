using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Utils;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ExceptionsThrownRelationVisitor :
    IExtractionVisitor<ClassBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceBlockSyntax, SemanticModel, IMembersClassType>
{
    public const string ExceptionsThrownDependencyMetricName = "ExceptionsThrownDependency";

    public IMembersClassType Visit(ClassBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var dictionary = AddDependencies(syntaxNode, semanticModel);

        modelType.Metrics.Add(new MetricModel(
            ExceptionsThrownDependencyMetricName,
            GetType().ToString(),
            dictionary.GetType().ToString(),
            dictionary
        ));

        return modelType;
    }

    public IMembersClassType Visit(StructureBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var dictionary = AddDependencies(syntaxNode, semanticModel);

        modelType.Metrics.Add(new MetricModel(
            ExceptionsThrownDependencyMetricName,
            GetType().ToString(),
            dictionary.GetType().ToString(),
            dictionary
        ));

        return modelType;
    }

    public IMembersClassType Visit(InterfaceBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var dictionary = AddDependencies(syntaxNode, semanticModel);

        modelType.Metrics.Add(new MetricModel(
            ExceptionsThrownDependencyMetricName,
            GetType().ToString(),
            dictionary.GetType().ToString(),
            dictionary
        ));

        return modelType;
    }

    private IDictionary<string, int> AddDependencies(TypeBlockSyntax syntaxNode, SemanticModel semanticModel)
    {
        var dictionary = new Dictionary<string, int>();

        foreach (var throwStatementSyntax in syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>())
        {
            if (throwStatementSyntax.Expression is null)
            {
                var catchBlockSyntax = throwStatementSyntax.GetParentDeclarationSyntax<CatchBlockSyntax>();
                if (catchBlockSyntax?.CatchStatement?.AsClause != null)
                {
                    AddDependency(dictionary,
                        VisualBasicExtractionHelperMethods
                            .GetFullName(catchBlockSyntax.CatchStatement.AsClause.Type, semanticModel).Name);
                }
            }
            else
            {
                AddDependency(dictionary,
                    VisualBasicExtractionHelperMethods.GetFullName(throwStatementSyntax.Expression, semanticModel)
                        .Name);
            }
        }

        return dictionary;
    }

    private static void AddDependency(IDictionary<string, int> dependencies, string dependencyName)
    {
        dependencyName = dependencyName.Trim('?');
        if (dependencies.ContainsKey(dependencyName))
        {
            dependencies[dependencyName]++;
        }
        else
        {
            dependencies.Add(dependencyName, 1);
        }
    }
}
