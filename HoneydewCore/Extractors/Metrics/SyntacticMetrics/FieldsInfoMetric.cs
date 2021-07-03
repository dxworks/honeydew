﻿using System.Collections.Generic;
using HoneydewCore.Models;
using HoneydewCore.Utils;
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
            var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
            var modifier = allModifiers;

            
            foreach (var m in CSharpConstants.AccessModifiers)
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