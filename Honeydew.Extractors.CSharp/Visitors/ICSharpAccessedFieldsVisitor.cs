﻿using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors;

public interface ICSharpAccessedFieldsVisitor : IAccessedFieldsVisitor,
    IExtractionVisitor<ExpressionSyntax, SemanticModel, AccessedField>
{
}