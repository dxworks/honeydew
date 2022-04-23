using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

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
