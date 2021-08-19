﻿using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Classes
{
    public interface ICSharpClassVisitor : IClassVisitor, ICSharpTypeVisitor,
        IVisitorType<BaseTypeDeclarationSyntax, IMembersClassType>
    {
    }
}
