using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit
{
    public class DelegateSetterCompilationUnitVisitor : CompositeVisitor, ICSharpCompilationUnitVisitor
    {
        public DelegateSetterCompilationUnitVisitor(IEnumerable<IDelegateVisitor> visitors) : base(visitors)
        {
        }

        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
        {
            foreach (var delegateDeclarationSyntax in syntaxNode.DescendantNodes().OfType<DelegateDeclarationSyntax>())
            {
                IDelegateType delegateModel = new DelegateModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    if (visitor is ICSharpDelegateVisitor extractionVisitor)
                    {
                        delegateModel = extractionVisitor.Visit(delegateDeclarationSyntax, delegateModel);
                    }
                }

                modelType.ClassTypes.Add(delegateModel);
            }

            return modelType;
        }
    }
}
