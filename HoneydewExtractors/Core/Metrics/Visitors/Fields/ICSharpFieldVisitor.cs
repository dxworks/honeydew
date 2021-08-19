﻿using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Fields
{
    public interface ICSharpFieldVisitor : IFieldVisitor, ICSharpTypeVisitor,
        IVisitorType<BaseFieldDeclarationSyntax, IFieldType>
    {
    }
}
