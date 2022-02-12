using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class;

public class IsAbstractClassVisitor : ICSharpClassVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        var isAbstract = false;

        if (syntaxNode is InterfaceDeclarationSyntax)
        {
            isAbstract = true;
        }
        else
        {
            foreach (var m in syntaxNode.Modifiers)
            {
                if (!CSharpExtractionHelperMethods.IsAbstractModifier(m.ValueText))
                {
                    continue;
                }

                isAbstract = true;

                break;
            }
        }

        modelType.Metrics.Add(new MetricModel
        {
            Value = isAbstract,
            ValueType = isAbstract.GetType().ToString(),
            ExtractorName = GetType().ToString()
        });

        return modelType;
    }
}
