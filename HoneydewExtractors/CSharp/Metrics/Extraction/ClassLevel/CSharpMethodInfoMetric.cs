using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        public CSharpMethodInfoDataMetric DataMetric { get; set; } = new();

        private string _containingClassName = "";

        private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<CSharpMethodInfoDataMetric>(DataMetric);
        }

        public override string PrettyPrint()
        {
            return "Methods Info";
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax syntax)
        {
            _containingClassName = HoneydewSemanticModel.GetFullName(syntax);

            var returnType = HoneydewSemanticModel.GetFullName(syntax.ReturnType);

            var methodModel = new MethodModel
            {
                Name = _containingClassName,
                ReturnType = new ReturnTypeModel
                {
                    Name = returnType
                },
                ContainingTypeName = _containingClassName,
                Modifier = "",
                AccessModifier = "",
                CyclomaticComplexity = 0
            };

            ExtractInfoAboutParameters(syntax.ParameterList, methodModel);

            DataMetric.MethodInfos.Add(methodModel);
        }

        private void ExtractInfoAboutParameters(BaseParameterListSyntax parameterList, IMethodSignatureType methodModel)
        {
            foreach (var parameter in parameterList.Parameters)
            {
                var parameterType = HoneydewSemanticModel.GetFullName(parameter.Type);

                methodModel.ParameterTypes.Add(new ParameterModel
                {
                    Name = parameterType,
                    Modifier = parameter.Modifiers.ToString(),
                    DefaultValue = parameter.Default?.Value.ToString()
                });
            }
        }
    }
}
