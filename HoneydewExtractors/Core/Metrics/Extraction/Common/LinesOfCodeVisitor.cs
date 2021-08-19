using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class LinesOfCodeVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpPropertyVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor
    {
        private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();

        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
            return modelType;
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
            return modelType;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
            return modelType;
        }
    }
}
