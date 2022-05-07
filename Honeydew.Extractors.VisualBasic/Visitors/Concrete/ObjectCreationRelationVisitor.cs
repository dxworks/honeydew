using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Utils;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ObjectCreationRelationVisitor :
    IExtractionVisitor<ClassBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceBlockSyntax, SemanticModel, IMembersClassType>
{
    public const string ObjectCreationDependencyMetricName = "ObjectCreationDependency";
    
    public IMembersClassType Visit(ClassBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var dictionary = AddDependencies(syntaxNode, semanticModel);

        modelType.Metrics.Add(new MetricModel(
            ObjectCreationDependencyMetricName,
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
            ObjectCreationDependencyMetricName,
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
            ObjectCreationDependencyMetricName,
            GetType().ToString(),
            dictionary.GetType().ToString(),
            dictionary
        ));

        return modelType;
    }

    private IDictionary<string, int> AddDependencies(SyntaxNode syntaxNode,
        SemanticModel semanticModel)
    {
        var dictionary = new Dictionary<string, int>();

        foreach (var objectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ObjectCreationExpressionSyntax>())
        {
            var dependencyName = VisualBasicExtractionHelperMethods
                .GetFullName(objectCreationExpressionSyntax.Type, semanticModel).Name;
            if (dependencyName != VisualBasicConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
            }
        }
        

        foreach (var arrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ArrayCreationExpressionSyntax>())
        {
            var dependencyName = VisualBasicExtractionHelperMethods
                .GetFullName(arrayCreationExpressionSyntax.Type, semanticModel).Name;
            if (dependencyName != VisualBasicConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
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
