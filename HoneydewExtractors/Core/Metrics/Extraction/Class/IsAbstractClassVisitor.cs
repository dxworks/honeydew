using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class IsAbstractClassVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpClassVisitor
    {
        public IMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IMembersClassType modelType)
        {
            // todo
            // if (syntaxNode is InterfaceDeclarationSyntax)
            // {
            //     modelType
            // }
            
            return modelType;
        }
    }
}
