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
    public class ClassSetterCompilationUnitVisitor : CompositeTypeVisitor, ICSharpCompilationUnitVisitor
    {
        private readonly CSharpClassModelCreator _cSharpClassTypeModelCreator;

        public ClassSetterCompilationUnitVisitor(CSharpClassModelCreator cSharpClassTypeModelCreator)
        {
            _cSharpClassTypeModelCreator = cSharpClassTypeModelCreator;

            foreach (var visitor in _cSharpClassTypeModelCreator.GetVisitors())
            {
                Add(visitor);
            }
        }

        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType compilationUnitType)
        {
            foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                compilationUnitType.ClassTypes.Add(
                    _cSharpClassTypeModelCreator.Create(baseTypeDeclarationSyntax, new ClassModel()));
            }

            return compilationUnitType;
        }
    }
}
