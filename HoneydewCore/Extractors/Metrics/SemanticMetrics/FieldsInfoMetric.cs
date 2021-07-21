using System.Collections.Generic;
using HoneydewCore.Models;
using HoneydewCore.Utils;
using HoneydewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class FieldsInfoMetric : CSharpMetricExtractor, ISemanticMetric
    {
        public IList<FieldModel> FieldInfos { get; } = new List<FieldModel>();

        public override IMetric GetMetric()
        {
            return new Metric<IList<FieldModel>>(FieldInfos);
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

            var typeName = node.Declaration.Type.ToString();
            var nodeSymbol = ExtractorSemanticModel.GetSymbolInfo(node.Declaration.Type).Symbol;
            if (nodeSymbol != null)
            {
                typeName = nodeSymbol.ToString();
            }
            
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
