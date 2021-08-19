using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxNode = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class LinesOfCodeVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpPropertyVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor, ICSharpClassVisitor,
        ICSharpCompilationUnitVisitor
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

        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode,
            IPropertyMembersClassType modelType)
        {
            modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
            return modelType;
        }

        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
        {
            var linesOfCode = _linesOfCodeCounter.Count(syntaxNode.ToString());

            if (syntaxNode.HasLeadingTrivia)
            {
                var loc = _linesOfCodeCounter.Count(syntaxNode.GetLeadingTrivia().ToString());
                linesOfCode.SourceLines += loc.SourceLines;
                linesOfCode.CommentedLines += loc.CommentedLines;
                linesOfCode.EmptyLines += loc.EmptyLines;
            }

            modelType.Loc = linesOfCode;
            return modelType;
        }
    }
}
