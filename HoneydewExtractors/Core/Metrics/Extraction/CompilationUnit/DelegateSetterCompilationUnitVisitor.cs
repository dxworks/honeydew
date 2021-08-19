using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit
{
    public class DelegateSetterCompilationUnitVisitor : CompositeTypeVisitor, ICSharpCompilationUnitVisitor
    {
        private readonly CSharpDelegateModelCreator _cSharpDelegateModelCreator;

        public DelegateSetterCompilationUnitVisitor(CSharpDelegateModelCreator cSharpDelegateModelCreator)
        {
            _cSharpDelegateModelCreator = cSharpDelegateModelCreator;

            foreach (var visitor in _cSharpDelegateModelCreator.GetVisitors())
            {
                Add(visitor);
            }
        }

        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
        {
            foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<DelegateDeclarationSyntax>())
            {
                modelType.ClassTypes.Add(
                    _cSharpDelegateModelCreator.Create(baseTypeDeclarationSyntax, new DelegateModel()));
            }

            return modelType;
        }
    }
}
