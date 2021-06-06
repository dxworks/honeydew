using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics
{
    public class UsingsCountMetric : CSharpMetricExtractor
    {
        private ICollection<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();

        public override string GetName()
        {
            return "Usings Count";
        }

        public override int GetMetric()
        {
            return Usings.Count;
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToString() != "System" && !node.Name.ToString().StartsWith("System."))
            {
                Usings.Add(node);
            }
        }
    }
}