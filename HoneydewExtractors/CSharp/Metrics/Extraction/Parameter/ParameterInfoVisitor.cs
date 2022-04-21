using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;

public class ParameterInfoVisitor : ICSharpParameterVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

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
