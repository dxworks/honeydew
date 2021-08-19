using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class BaseInfoClassVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpClassVisitor
    {
        public IMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IMembersClassType modelType)
        {
            modelType.Name = InheritedSemanticModel.GetFullName(syntaxNode);

            return modelType;
        }
    }
}
