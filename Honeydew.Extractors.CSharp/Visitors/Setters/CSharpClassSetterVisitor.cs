﻿using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpClassSetterVisitor :
    CompositeVisitor<IMembersClassType>,
    IClassSetterCompilationUnitVisitor<CompilationUnitSyntax, SemanticModel, TypeDeclarationSyntax>
{
    public CSharpClassSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IMembersClassType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMembersClassType CreateWrappedType() => new CSharpClassModel();

    public IEnumerable<TypeDeclarationSyntax> GetWrappedSyntaxNodes(CompilationUnitSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<TypeDeclarationSyntax>();
    }
}
