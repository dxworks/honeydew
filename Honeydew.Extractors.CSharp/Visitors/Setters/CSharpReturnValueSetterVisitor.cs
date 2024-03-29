﻿using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors.Extraction;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpReturnValueSetterVisitor :
    CompositeVisitor<IReturnValueType>,
    IReturnValueSetterVisitor<DelegateDeclarationSyntax, SemanticModel, TypeSyntax, IDelegateType>,
    IReturnValueSetterVisitor<MethodDeclarationSyntax, SemanticModel, TypeSyntax, IMethodType>,
    IReturnValueSetterVisitor<ArrowExpressionClauseSyntax, SemanticModel, TypeSyntax, IAccessorMethodType>,
    IReturnValueSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, TypeSyntax, IMethodTypeWithLocalFunctions>
{
    public CSharpReturnValueSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IReturnValueType>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IReturnValueType CreateWrappedType() => new CSharpReturnValueModel();

    public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(DelegateDeclarationSyntax syntaxNode)
    {
        yield return syntaxNode.ReturnType;
    }

    public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        yield return syntaxNode.ReturnType;
    }

    public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(ArrowExpressionClauseSyntax syntaxNode)
    {
        var basePropertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (basePropertyDeclarationSyntax != null)
        {
            yield return basePropertyDeclarationSyntax.Type;
        }
    }

    public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        yield return syntaxNode.ReturnType;
    }
}
