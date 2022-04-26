using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Honeydew.Extractors.Dotnet;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public record AccessorReturnValue(string Type, TypeSyntax ReturnType);

public partial class CSharpReturnValueSetterVisitor :
    IReturnValueSetterVisitor<AccessorDeclarationSyntax, SemanticModel, AccessorReturnValue, IAccessorMethodType>

{
    public IEnumerable<AccessorReturnValue> GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        // only for getters the return type is correct 

        var basePropertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (basePropertyDeclarationSyntax != null)
        {
            yield return new AccessorReturnValue(syntaxNode.Keyword.ToString(), basePropertyDeclarationSyntax.Type);
        }
    }
}
