using System.Collections.Generic;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SyntacticMetrics
{
    public class FieldsInfoMetric : CSharpMetricExtractor, ISyntacticMetric
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
            var accessModifier = "private";
            var modifier = allModifiers;

            var allAccessModifiers = new[]
                {"private protected", "protected internal", "public", "private", "protected", "internal"};
            foreach (var m in allAccessModifiers)
            {
                if (!allModifiers.Contains(m)) continue;

                accessModifier = m;
                modifier = allModifiers.Replace(m, "").Trim();
                break;
            }

            foreach (var variable in node.Declaration.Variables)
            {
                FieldInfos.Add(new FieldModel
                {
                    AccessModifier = accessModifier,
                    Modifier = modifier,
                    IsEvent = isEvent,
                    Type = node.Declaration.Type.ToString(),
                    Name = variable.Identifier.ToString(),
                });
            }
        }
    }
}