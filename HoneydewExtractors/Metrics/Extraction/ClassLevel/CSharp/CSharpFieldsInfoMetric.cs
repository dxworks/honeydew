using System.Collections.Generic;
using HoneydewExtractors.Metrics.CSharp;
using HoneydewExtractors.Utils;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp
{
    public class CSharpFieldsInfoMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>, IFieldsInfoMetric
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }


        public IList<FieldModel> FieldInfos { get; } = new List<FieldModel>();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<IList<FieldModel>>(FieldInfos);
        }

        public override string PrettyPrint()
        {
            return "Fields Info";
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            AddFieldInfo(node, true);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            AddFieldInfo(node, false);
        }

        private void AddFieldInfo(BaseFieldDeclarationSyntax node, bool isEvent)
        {
            var allModifiers = node.Modifiers.ToString();
            var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
            var modifier = allModifiers;
            
            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

            var typeName = HoneydewSemanticModel.GetFullName(node.Declaration);

            foreach (var variable in node.Declaration.Variables)
            {
                FieldInfos.Add(new FieldModel
                {
                    AccessModifier = accessModifier,
                    Modifier = modifier,
                    IsEvent = isEvent,
                    Type = typeName,
                    Name = variable.Identifier.ToString(),
                });
            }
        }
    }
}
