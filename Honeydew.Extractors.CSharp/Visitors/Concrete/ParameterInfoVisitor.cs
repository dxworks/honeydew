using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ParameterInfoVisitor : IExtractionVisitor<ParameterSyntax, SemanticModel, IParameterType>
{
    public IParameterType Visit(ParameterSyntax syntaxNode, SemanticModel semanticModel, IParameterType modelType)
    {
        var parameterInfo = CSharpExtractionHelperMethods.ExtractInfoAboutParameter(syntaxNode, semanticModel);
        if (parameterInfo is null)
        {
            return modelType;
        }

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
