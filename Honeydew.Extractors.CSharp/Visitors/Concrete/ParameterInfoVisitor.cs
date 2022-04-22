﻿using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ParameterInfoVisitor : ICSharpParameterVisitor
{
    public IParameterType Visit(ParameterSyntax syntaxNode, SemanticModel semanticModel, IParameterType modelType)
    {
        var parameterInfo = CSharpExtractionHelperMethods.ExtractInfoAboutParameter(syntaxNode, semanticModel);
        modelType.Type = parameterInfo.Type;
        modelType.IsNullable = parameterInfo.IsNullable;

        if (modelType is not ParameterModel parameterModel)
        {
            return modelType;
        }

        parameterModel.Modifier = parameterInfo.Modifier;
        parameterModel.DefaultValue = parameterInfo.DefaultValue;
        return parameterModel;
    }
}