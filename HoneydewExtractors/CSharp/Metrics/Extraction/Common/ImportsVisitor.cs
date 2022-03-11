using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common;

public class ImportsVisitor : ICSharpCompilationUnitVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
    ICSharpEnumVisitor
{
    public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        var usingSet = new HashSet<string>();

        foreach (var usingDirectiveSyntax in syntaxNode.DescendantNodes().OfType<UsingDirectiveSyntax>())
        {
            var usingName = usingDirectiveSyntax.Name.ToString();

            if (usingSet.Contains(usingName))
            {
                continue;
            }

            usingSet.Add(usingName);

            var usingModel = GetUsingModelFromUsingSyntax(usingDirectiveSyntax, semanticModel);
            modelType.Imports.Add(usingModel);
        }

        return modelType;
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMembersClassType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode, semanticModel))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode, semanticModel))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    public IEnumType Visit(EnumDeclarationSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode, semanticModel))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    private List<IImportType> ExtractParentImports(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var usingSet = new HashSet<string>();
        var importTypes = new List<IImportType>();

        var parent = syntaxNode.Parent;

        while (parent != null)
        {
            foreach (var usingDirectiveSyntax in parent.ChildNodes().OfType<UsingDirectiveSyntax>())
            {
                var usingName = usingDirectiveSyntax.Name.ToString();

                if (usingSet.Contains(usingName))
                {
                    continue;
                }

                usingSet.Add(usingName);
                var usingModel = GetUsingModelFromUsingSyntax(usingDirectiveSyntax, semanticModel);
                importTypes.Add(usingModel);
            }

            parent = parent.Parent;
        }

        return importTypes;
    }

    private UsingModel GetUsingModelFromUsingSyntax(UsingDirectiveSyntax usingDirectiveSyntax,
        SemanticModel semanticModel)
    {
        var usingName = usingDirectiveSyntax.Name.ToString();
        var isStatic = usingDirectiveSyntax.StaticKeyword.Value != null;
        var alias = usingDirectiveSyntax.Alias == null ? "" : usingDirectiveSyntax.Alias.Name.ToString();
        var aliasType = nameof(EAliasType.None);

        if (!string.IsNullOrEmpty(alias))
        {
            aliasType = CSharpExtractionHelperMethods.GetAliasTypeOfNamespace(usingDirectiveSyntax.Name,
                semanticModel);
        }

        var usingModel = new UsingModel
        {
            Name = usingName,
            IsStatic = isStatic,
            Alias = alias,
            AliasType = aliasType
        };
        return usingModel;
    }

    public void Accept(IVisitor visitor)
    {
    }
}
