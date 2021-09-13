using System.Linq;
using HoneydewExtractors.Core;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpFactExtractor : IFactExtractor
    {
        private readonly ICompositeVisitor _compositeVisitor;

        public CSharpFactExtractor(ICompositeVisitor compositeVisitor)
        {
            _compositeVisitor = compositeVisitor;
        }

        public ICompilationUnitType Extract(SyntaxTree syntacticTree, SemanticModel semanticModel)
        {
            var syntacticModel = new CSharpSyntacticModel
            {
                Tree = syntacticTree,
                CompilationUnitSyntax = GetCompilationUnitSyntaxTree(syntacticTree)
            };
            var cSharpSemanticModel = new CSharpSemanticModel
            {
                Model = semanticModel
            };

            IVisitor extractionModelsSetterVisitor =
                new ExtractionModelsSetterVisitor(new CSharpExtractionHelperMethods(cSharpSemanticModel));

            _compositeVisitor.Accept(extractionModelsSetterVisitor);

            ICompilationUnitType compilationUnitModel = new CompilationUnitModel();

            foreach (var visitor in _compositeVisitor.GetContainedVisitors())
            {
                if (visitor is ICSharpCompilationUnitVisitor compilationUnitVisitor)
                {
                    compilationUnitModel =
                        compilationUnitVisitor.Visit(syntacticModel.CompilationUnitSyntax, compilationUnitModel);
                }
            }

            return compilationUnitModel;
        }

        private static CompilationUnitSyntax GetCompilationUnitSyntaxTree(SyntaxTree tree)
        {
            var root = tree.GetCompilationUnitRoot();

            var diagnostics = root.GetDiagnostics();

            var enumerable = diagnostics as Diagnostic[] ?? diagnostics.ToArray();
            if (diagnostics != null && enumerable.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var result = enumerable.Aggregate("", (current, diagnostic) => current + diagnostic);
                throw new ExtractionException(result);
            }

            return root;
        }
    }
}
