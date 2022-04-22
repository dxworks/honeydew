using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

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
