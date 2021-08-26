using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Parameter
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
