using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class MethodSetterClassVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpClassVisitor
    {
        private readonly CSharpMethodModelCreator _cSharpMethodModelCreator;

        public MethodSetterClassVisitor(CSharpMethodModelCreator cSharpMethodModelCreator)
        {
            _cSharpMethodModelCreator = cSharpMethodModelCreator;
        }

        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IPropertyMembersClassType classType)
        {
            foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                classType.Methods.Add(_cSharpMethodModelCreator.Create(baseTypeDeclarationSyntax, new MethodModel()));
            }

            return classType;
        }
    }
}
