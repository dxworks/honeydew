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

public class DelegateSetterCompilationUnitVisitor : CompositeVisitor, ICSharpCompilationUnitVisitor
{
    public DelegateSetterCompilationUnitVisitor(IEnumerable<IDelegateVisitor> visitors) : base(visitors)
    {
    }

    public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        foreach (var delegateDeclarationSyntax in syntaxNode.DescendantNodes().OfType<DelegateDeclarationSyntax>())
        {
            IDelegateType delegateModel = new DelegateModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpDelegateVisitor extractionVisitor)
                    {
                        delegateModel =
                            extractionVisitor.Visit(delegateDeclarationSyntax, semanticModel, delegateModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Delegate Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.ClassTypes.Add(delegateModel);
        }

        return modelType;
    }
}
