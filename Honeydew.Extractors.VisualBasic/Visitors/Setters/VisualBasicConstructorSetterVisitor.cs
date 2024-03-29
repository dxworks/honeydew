﻿using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicConstructorSetterVisitor :
    CompositeVisitor<IConstructorType>,
    IConstructorSetterClassVisitor<ClassBlockSyntax, SemanticModel, ConstructorBlockSyntax>
{
    public VisualBasicConstructorSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IConstructorType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IConstructorType CreateWrappedType() => new VisualBasicConstructorModel();

    public IEnumerable<ConstructorBlockSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<ConstructorBlockSyntax>();
    }
}
