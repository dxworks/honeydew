using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Destructor;

public class DestructorInfoVisitor : ICSharpDestructorVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel, IDestructorType modelType)
    {
        var containingClassName = "";
        if (syntaxNode.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            containingClassName = CSharpExtractionHelperMethods.GetFullName(baseTypeDeclarationSyntax, semanticModel).Name;
        }

        modelType.Name = $"~{syntaxNode.Identifier.ToString()}";
        modelType.ContainingTypeName = containingClassName;
        modelType.Modifier = "";
        modelType.AccessModifier = "";
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }
}
