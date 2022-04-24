using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp;

public class CSharpFactExtractor : IFactExtractor
{
    private readonly CompositeVisitor<ICompilationUnitType> _compositeVisitor;

    public CSharpFactExtractor(CompositeVisitor<ICompilationUnitType> compositeVisitor)
    {
        _compositeVisitor = compositeVisitor;
    }

    public ICompilationUnitType Extract(SyntaxTree syntacticTree, SemanticModel semanticModel)
    {
        var compilationUnitSyntaxTree = GetCompilationUnitSyntaxTree(syntacticTree);

        ICompilationUnitType compilationUnitModel = new CSharpCompilationUnitModel();

        foreach (var visitor in _compositeVisitor.GetContainedVisitors())
        {
            if (visitor is IExtractionVisitor<CompilationUnitSyntax, SemanticModel, ICompilationUnitType>
                compilationUnitVisitor)
            {
                compilationUnitModel =
                    compilationUnitVisitor.Visit(compilationUnitSyntaxTree, semanticModel, compilationUnitModel);
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
