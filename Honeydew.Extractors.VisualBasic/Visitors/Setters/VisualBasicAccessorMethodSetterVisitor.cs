using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicAccessorMethodSetterVisitor :
    CompositeVisitor<IAccessorMethodType>,
    IAccessorMethodSetterPropertyVisitor<PropertyStatementSyntax, SemanticModel, AccessorBlockSyntax>
{
    public VisualBasicAccessorMethodSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IAccessorMethodType>> visitors) : base(compositeLogger,
        visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IAccessorMethodType CreateWrappedType() => new VisualBasicAccessorMethodModel();

    IEnumerable<AccessorBlockSyntax>
        ISetterVisitor<PropertyStatementSyntax, SemanticModel, IPropertyType, AccessorBlockSyntax,
            IAccessorMethodType>.GetWrappedSyntaxNodes(PropertyStatementSyntax syntaxNode)
    {
        if (syntaxNode.Parent is PropertyBlockSyntax propertyBlock)
        {
            return propertyBlock.Accessors;
        }

        return Enumerable.Empty<AccessorBlockSyntax>();
    }

    public IPropertyType Visit(PropertyStatementSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        var accessorDeclarationSyntaxNodes =
            ((IAccessorMethodSetterPropertyVisitor<PropertyStatementSyntax, SemanticModel, AccessorBlockSyntax>)this)
            .GetWrappedSyntaxNodes(syntaxNode);
        foreach (var wrappedSyntaxNode in accessorDeclarationSyntaxNodes)
        {
            var accessorMethodType =
                ((IAccessorMethodSetterPropertyVisitor<PropertyStatementSyntax, SemanticModel, AccessorBlockSyntax>)
                    this).ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.Accessors.Add(accessorMethodType);
        }

        return modelType;
    }
}
