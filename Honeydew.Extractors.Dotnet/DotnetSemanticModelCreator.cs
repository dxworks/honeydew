using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.Dotnet;

public class DotnetSemanticModelCreator
{
    private readonly ICompilationMaker _compilationMaker;

    public DotnetSemanticModelCreator(ICompilationMaker compilationMaker)
    {
        _compilationMaker = compilationMaker;
    }

    public SemanticModel Create(SyntaxTree tree)
    {
        var compilation = _compilationMaker.GetCompilation();

        compilation = compilation.AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);
        return semanticModel;
    }
}
