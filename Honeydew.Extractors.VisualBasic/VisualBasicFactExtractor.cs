using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic;

public class VisualBasicFactExtractor : IFactExtractor
{
    private readonly CompositeVisitor<ICompilationUnitType> _compositeVisitor;
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new VisualBasicCompilationMaker());

    public VisualBasicFactExtractor(CompositeVisitor<ICompilationUnitType> compositeVisitor)
    {
        _compositeVisitor = compositeVisitor;
    }

    public ICompilationUnitType Extract(SyntaxTree syntacticTree, SemanticModel semanticModel)
    {
        var compilationUnitSyntaxTree = GetCompilationUnitSyntaxTree(syntacticTree);

        ICompilationUnitType compilationUnitModel = new VisualBasicCompilationUnitModel();

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

    public async Task<ICompilationUnitType> Extract(string filePath, CancellationToken cancellationToken)
    {
        var fileContent = await File.ReadAllTextAsync(filePath, cancellationToken);
        var syntaxTree = VisualBasicSyntaxTree.ParseText(fileContent, cancellationToken: cancellationToken);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        return Extract(syntaxTree, semanticModel);
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
