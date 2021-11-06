﻿using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.AccessedFields
{
    public interface ICSharpAccessedFieldsVisitor : IAccessedFieldsVisitor,
        IExtractionVisitor<ExpressionSyntax, AccessedField>
    {
    }
}