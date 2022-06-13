using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpAccessorMethodSetterVisitor :
    CompositeVisitor<IAccessorMethodType>,
    IAccessorMethodSetterPropertyVisitor<BasePropertyDeclarationSyntax, SemanticModel, AccessorDeclarationSyntax>,
    IAccessorMethodSetterPropertyVisitor<BasePropertyDeclarationSyntax, SemanticModel, ArrowExpressionClauseSyntax>
{
    public CSharpAccessorMethodSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IAccessorMethodType>> visitors) : base(compositeLogger,
        visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IAccessorMethodType CreateWrappedType() => new CSharpAccessorMethodModel();

    IEnumerable<AccessorDeclarationSyntax>
        ISetterVisitor<BasePropertyDeclarationSyntax, SemanticModel, IPropertyType, AccessorDeclarationSyntax,
            IAccessorMethodType>.GetWrappedSyntaxNodes(BasePropertyDeclarationSyntax syntaxNode)
    {
        return syntaxNode.AccessorList?.Accessors ?? Enumerable.Empty<AccessorDeclarationSyntax>();
    }

    IEnumerable<ArrowExpressionClauseSyntax>
        ISetterVisitor<BasePropertyDeclarationSyntax, SemanticModel, IPropertyType, ArrowExpressionClauseSyntax,
            IAccessorMethodType>.GetWrappedSyntaxNodes(BasePropertyDeclarationSyntax syntaxNode)
    {
        if (syntaxNode.AccessorList is null &&
            syntaxNode is PropertyDeclarationSyntax { ExpressionBody: { } } propertyDeclarationSyntax)
        {
            return new[] { propertyDeclarationSyntax.ExpressionBody };
        }

        return Enumerable.Empty<ArrowExpressionClauseSyntax>();
    }

    public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        var accessorDeclarationSyntaxNodes =
            ((IAccessorMethodSetterPropertyVisitor<BasePropertyDeclarationSyntax, SemanticModel,
                AccessorDeclarationSyntax>)this)
            .GetWrappedSyntaxNodes(syntaxNode);
        foreach (var wrappedSyntaxNode in accessorDeclarationSyntaxNodes)
        {
            var accessorMethodType =
                ((IAccessorMethodSetterPropertyVisitor<BasePropertyDeclarationSyntax, SemanticModel,
                    AccessorDeclarationSyntax>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Accessors.Add(accessorMethodType);
        }

        var arrowExpressionClauseSyntaxNodes =
            ((ISetterVisitor<BasePropertyDeclarationSyntax, SemanticModel, IPropertyType, ArrowExpressionClauseSyntax,
                IAccessorMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);
        foreach (var wrappedSyntaxNode in arrowExpressionClauseSyntaxNodes)
        {
            var accessorMethodType =
                ((ISetterVisitor<BasePropertyDeclarationSyntax, SemanticModel, IPropertyType,
                    ArrowExpressionClauseSyntax, IAccessorMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Accessors.Add(accessorMethodType);
        }

        return modelType;
    }
}
