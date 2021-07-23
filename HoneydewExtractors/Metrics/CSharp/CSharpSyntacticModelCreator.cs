using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.CSharp
{
    public class CSharpSyntacticModelCreator : ISyntacticModelCreator<CSharpSyntacticModel>
    {
        public CSharpSyntacticModel Create(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new ExtractionException("Empty Content");
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            var root = GetCompilationUnitSyntaxTree(tree);

            return new CSharpSyntacticModel
            {
                Tree = tree,
                CompilationUnitSyntax = root
            };
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
