using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Delegate
{
    public class ContainingTypeDelegateVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpDelegateVisitor
    {
        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            modelType.ContainingTypeName = InheritedSemanticModel.GetFullName(syntaxNode)
                .Replace(syntaxNode.Identifier.ToString(), "").Trim('.');

            return modelType;
        }
    }
}
