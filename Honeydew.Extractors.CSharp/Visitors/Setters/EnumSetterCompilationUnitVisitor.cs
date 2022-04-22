using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class EnumSetterCompilationUnitVisitor : CompositeVisitor, ICSharpCompilationUnitVisitor
{
    public EnumSetterCompilationUnitVisitor(IEnumerable<IEnumVisitor> visitors) : base(visitors)
    {
    }

    public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            IEnumType enumModel = new EnumModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpEnumVisitor extractionVisitor)
                    {
                        enumModel = extractionVisitor.Visit(baseTypeDeclarationSyntax, semanticModel, enumModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Enum Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.ClassTypes.Add(enumModel);
        }

        return modelType;
    }
}
