using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit
{
    public class ClassSetterCompilationUnitVisitor : CompositeVisitor, ICSharpCompilationUnitVisitor
    {
        public ClassSetterCompilationUnitVisitor(IEnumerable<IClassVisitor> visitors) : base(visitors)
        {
        }

        public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, ICompilationUnitType modelType)
        {
            foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                IClassType classModel = new ClassModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    if (visitor is ICSharpClassVisitor extractionVisitor)
                    {
                        classModel = extractionVisitor.Visit(baseTypeDeclarationSyntax, classModel);
                    }
                }

                modelType.ClassTypes.Add(classModel);
            }

            return modelType;
        }
    }
}
