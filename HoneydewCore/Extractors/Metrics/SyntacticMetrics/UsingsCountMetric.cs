﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SyntacticMetrics
{
    public class UsingsCountMetric : CSharpMetricExtractor, ISyntacticMetric
    {
        public int UsingsCount { get; private set; }

        public override IMetric GetMetric()
        {
            return new Metric<int>(UsingsCount);
        }

        public override string PrettyPrint()
        {
            return "Usings Count";
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            UsingsCount++;
        }
    }
}