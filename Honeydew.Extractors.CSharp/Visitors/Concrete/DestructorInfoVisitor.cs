﻿using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class DestructorInfoVisitor : IExtractionVisitor<DestructorDeclarationSyntax, SemanticModel, IDestructorType>
{
    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        modelType.Name = $"~{syntaxNode.Identifier.ToString()}";
        modelType.Modifier = "";
        modelType.AccessModifier = "";
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }
}
