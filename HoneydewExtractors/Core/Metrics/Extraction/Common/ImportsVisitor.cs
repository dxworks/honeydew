using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxNode = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class ImportsVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpCompilationUnitVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor
    {
        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
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

                var usingModel = GetUsingModelFromUsingSyntax(usingDirectiveSyntax);
                modelType.Imports.Add(usingModel);
            }

            return modelType;
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            foreach (var importType in ExtractParentImports(syntaxNode))
            {
                modelType.Imports.Add(importType);
            }

            return modelType;
        }

        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            foreach (var importType in ExtractParentImports(syntaxNode))
            {
                modelType.Imports.Add(importType);
            }

            return modelType;
        }

        private List<IImportType> ExtractParentImports(SyntaxNode syntaxNode)
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
                    var usingModel = GetUsingModelFromUsingSyntax(usingDirectiveSyntax);
                    importTypes.Add(usingModel);
                }

                parent = parent.Parent;
            }

            return importTypes;
        }

        private UsingModel GetUsingModelFromUsingSyntax(UsingDirectiveSyntax usingDirectiveSyntax)
        {
            var usingName = usingDirectiveSyntax.Name.ToString();
            var isStatic = usingDirectiveSyntax.StaticKeyword.Value != null;
            var alias = usingDirectiveSyntax.Alias == null ? "" : usingDirectiveSyntax.Alias.Name.ToString();
            var aliasType = nameof(EAliasType.None);

            if (!string.IsNullOrEmpty(alias))
            {
                aliasType = InheritedSemanticModel.GetAliasTypeOfNamespace(usingDirectiveSyntax.Name);
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
    }
}
