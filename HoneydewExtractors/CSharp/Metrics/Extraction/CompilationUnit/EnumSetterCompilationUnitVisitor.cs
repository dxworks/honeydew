using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;

public class EnumSetterCompilationUnitVisitor : CompositeVisitor, ICSharpCompilationUnitVisitor
{
    public EnumSetterCompilationUnitVisitor(IEnumerable<IEnumVisitor> visitors) : base(visitors)
    {
    }

    public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            IEnumType enumModel = new EnumModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpEnumVisitor extractionVisitor)
                    {
                        enumModel = extractionVisitor.Visit(baseTypeDeclarationSyntax, semanticModel, enumModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Enum Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.ClassTypes.Add(enumModel);
        }

        return modelType;
    }
}
