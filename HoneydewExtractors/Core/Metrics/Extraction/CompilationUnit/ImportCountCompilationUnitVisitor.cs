using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit
{
    public class ImportCountCompilationUnitVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpCompilationUnitVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
        {
            var importsCount = syntaxNode.DescendantNodes().OfType<UsingDirectiveSyntax>().Count();

            modelType.Metrics.Add(new MetricModel
            {
                Value = importsCount,
                ValueType = importsCount.GetType().ToString(),
                ExtractorName = GetType().ToString()
            });

            return modelType;
        }
    }
}
