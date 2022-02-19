using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HoneydewExtractors.CSharp.Metrics.Extraction.CSharpExtractionHelperMethods;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;

public class AttributeInfoVisitor : ICSharpAttributeVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

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
