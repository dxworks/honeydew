using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ImportsVisitor :
    IExtractionVisitor<CompilationUnitSyntax, SemanticModel, ICompilationUnitType>,
    IExtractionVisitor<TypeDeclarationSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<DelegateDeclarationSyntax, SemanticModel, IDelegateType>,
    IExtractionVisitor<EnumDeclarationSyntax, SemanticModel, IEnumType>
{
    public ICompilationUnitType Visit(CompilationUnitSyntax syntaxNode, SemanticModel semanticModel,
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

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
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

    private CSharpUsingModel GetUsingModelFromUsingSyntax(UsingDirectiveSyntax usingDirectiveSyntax,
        SemanticModel semanticModel)
    {
        var usingName = usingDirectiveSyntax.Name.ToString();
        var isStatic = usingDirectiveSyntax.StaticKeyword.Value != null;
        var alias = usingDirectiveSyntax.Alias == null ? "" : usingDirectiveSyntax.Alias.Name.ToString();
        var aliasType = nameof(EAliasType.None);

        if (!string.IsNullOrEmpty(alias))
        {
            aliasType = CSharpExtractionHelperMethods
                .GetAliasTypeOfNamespace(usingDirectiveSyntax.Name,
                    semanticModel);
        }

        var usingModel = new CSharpUsingModel
        {
            Name = usingName,
            IsStatic = isStatic,
            Alias = alias,
            AliasType = aliasType
        };
        return usingModel;
    }
}
