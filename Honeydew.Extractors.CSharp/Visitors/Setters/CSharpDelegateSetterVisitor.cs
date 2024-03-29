﻿using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpDelegateSetterVisitor :
    CompositeVisitor<IDelegateType>,
    IDelegateSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, DelegateDeclarationSyntax>
{
    public CSharpDelegateSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IDelegateType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IDelegateType CreateWrappedType() => new CSharpDelegateModel();

    public IEnumerable<DelegateDeclarationSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<DelegateDeclarationSyntax>();
    }
}
