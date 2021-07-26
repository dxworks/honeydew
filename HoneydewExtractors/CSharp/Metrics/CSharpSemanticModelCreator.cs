using HoneydewExtractors.Core;
using HoneydewExtractors.Core.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSemanticModelCreator : ISemanticModelCreator<CSharpSyntacticModel, CSharpSemanticModel>
    {
        private readonly ICompilationMaker _compilationMaker;

        public CSharpSemanticModelCreator(ICompilationMaker compilationMaker)
        {
            _compilationMaker = compilationMaker;
        }

        public CSharpSemanticModel Create(CSharpSyntacticModel syntacticModel)
        {
            var semanticModel = CreateSemanticModel(syntacticModel?.Tree);
            return new CSharpSemanticModel
            {
                Model = semanticModel
            };
        }

        private SemanticModel CreateSemanticModel(SyntaxTree tree)
        {
            var compilation = _compilationMaker.GetCompilation();

            compilation = compilation.AddSyntaxTrees(tree);

            var semanticModel = compilation.GetSemanticModel(tree);
            return semanticModel;
        }
    }
}
