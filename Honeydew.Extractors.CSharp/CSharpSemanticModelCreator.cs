﻿using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.CSharp;

public class CSharpSemanticModelCreator
{
    private readonly ICompilationMaker _compilationMaker;

    public CSharpSemanticModelCreator(ICompilationMaker compilationMaker)
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
