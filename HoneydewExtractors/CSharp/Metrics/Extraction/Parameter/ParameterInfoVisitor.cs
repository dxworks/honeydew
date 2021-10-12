using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Parameter
{
    public class ParameterInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpParameterVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IParameterType Visit(ParameterSyntax syntaxNode, IParameterType modelType)
        {
            var parameterInfo = CSharpHelperMethods.ExtractInfoAboutParameter(syntaxNode);
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
}
