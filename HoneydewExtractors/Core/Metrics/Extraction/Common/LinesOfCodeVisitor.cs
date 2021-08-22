using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class LinesOfCodeVisitor : ICSharpPropertyVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
        ICSharpClassVisitor, ICSharpCompilationUnitVisitor, ICSharpLocalFunctionVisitor
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

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, IMethodTypeWithLocalFunctions modelType)
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
