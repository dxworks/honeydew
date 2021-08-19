using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class PropertySetterClassVisitor : CompositeTypeVisitor, ICSharpClassVisitor
    {
        private readonly CSharpPropertyModelCreator _cSharpPropertyModelCreator;

        public PropertySetterClassVisitor(CSharpPropertyModelCreator cSharpPropertyModelCreator)
        {
            _cSharpPropertyModelCreator = cSharpPropertyModelCreator;

            foreach (var visitor in _cSharpPropertyModelCreator.GetVisitors())
            {
                Add(visitor);
            }
        }

        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IPropertyMembersClassType modelType)
        {
            foreach (var basePropertyDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<BasePropertyDeclarationSyntax>())
            {
                modelType.Properties.Add(
                    _cSharpPropertyModelCreator.Create(basePropertyDeclarationSyntax, new PropertyModel()));
            }

            return modelType;
        }
    }
}
