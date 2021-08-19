using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class ConstructorSetterClassVisitor : CompositeTypeVisitor, ICSharpClassVisitor
    {
        private readonly CSharpConstructorMethodModelCreator _cSharpConstructorMethodModelCreator;

        public ConstructorSetterClassVisitor(CSharpConstructorMethodModelCreator cSharpConstructorMethodModelCreator)
        {
            _cSharpConstructorMethodModelCreator = cSharpConstructorMethodModelCreator;

            foreach (var visitor in _cSharpConstructorMethodModelCreator.GetVisitors())
            {
                Add(visitor);
            }
        }

        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode,
            IPropertyMembersClassType modelType)
        {
            foreach (var constructorDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>())
            {
                modelType.Constructors.Add(
                    _cSharpConstructorMethodModelCreator.Create(constructorDeclarationSyntax, new ConstructorModel()));
            }

            return modelType;
        }
    }
}
