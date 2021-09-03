﻿using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.LocalVariables
{
    public interface ICSharpLocalVariablesVisitor : ILocalVariablesVisitor,
        IExtractionVisitor<VariableDeclaratorSyntax, ILocalVariableType>,
        IExtractionVisitor<DeclarationPatternSyntax, ILocalVariableType>,
        IExtractionVisitor<ForEachStatementSyntax, ILocalVariableType>
    {
    }
}