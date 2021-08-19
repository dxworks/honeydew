﻿using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Classes
{
    public interface ICSharpDelegateVisitor : IDelegateVisitor, ICSharpTypeVisitor,
        IVisitorType<DelegateDeclarationSyntax, IDelegateType>
    {
    }
}