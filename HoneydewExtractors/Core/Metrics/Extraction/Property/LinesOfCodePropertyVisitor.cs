using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Property
{
    public class LinesOfCodePropertyVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpPropertyVisitor
    {
        private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();

        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());

            return modelType;
        }
    }
}
