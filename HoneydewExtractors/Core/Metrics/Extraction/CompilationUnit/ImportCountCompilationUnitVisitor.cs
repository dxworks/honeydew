using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxNode = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode;

namespace HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit
{
    public class ImportCountCompilationUnitVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpCompilationUnitVisitor
    {
        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
        {
            var importsCount = syntaxNode.DescendantNodes().OfType<UsingDirectiveSyntax>().Count();
            
            modelType.Metrics.Add(new MetricModel
            {
                Value = importsCount,
                ValueType = importsCount.GetType().FullName,
                ExtractorName = GetType().FullName
            });
            
            return modelType;
        }
    }
}
