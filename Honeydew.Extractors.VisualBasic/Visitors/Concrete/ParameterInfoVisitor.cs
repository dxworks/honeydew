using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ParameterInfoVisitor : IExtractionVisitor<ParameterSyntax, SemanticModel, IParameterType>
{
    public IParameterType Visit(ParameterSyntax syntaxNode, SemanticModel semanticModel, IParameterType modelType)
    {
        var parameterType = GetFullName(syntaxNode.AsClause.Type, semanticModel, out var isNullable);

        modelType.Type = parameterType;
        modelType.IsNullable = isNullable;

        if (modelType is not VisualBasicParameterModel parameterModel)
        {
            return modelType;
        }

        parameterModel.Modifier = syntaxNode.Modifiers.ToString();
        parameterModel.DefaultValue = syntaxNode.Default?.Value.ToString();
        return parameterModel;
    }
}
