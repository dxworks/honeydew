using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;
namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class AttributeInfoVisitor : IExtractionVisitor<AttributeSyntax, SemanticModel, IAttributeType>
{
    public IAttributeType Visit(AttributeSyntax syntaxNode, SemanticModel semanticModel, IAttributeType modelType)
    {
        var fullNameType = GetFullName(syntaxNode, semanticModel);
        modelType.Name = fullNameType.Name;
        modelType.Type = fullNameType;
        var attributeTarget = GetAttributeTarget(syntaxNode);
        if (!string.IsNullOrEmpty(attributeTarget))
        {
            modelType.Target = attributeTarget;
        }

        foreach (var parameterType in GetParameters(syntaxNode, semanticModel))
        {
            modelType.ParameterTypes.Add(parameterType);
        }

        return modelType;
    }
}
