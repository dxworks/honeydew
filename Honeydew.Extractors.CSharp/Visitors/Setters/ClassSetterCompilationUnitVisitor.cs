using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class ClassSetterCompilationUnitVisitor : CompositeVisitor, ICSharpCompilationUnitVisitor
{
    public ClassSetterCompilationUnitVisitor(ILogger logger, IEnumerable<IClassVisitor> visitors) : base(logger,
        visitors)
    {
    }

    public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        foreach (var baseTypeDeclarationSyntax in syntaxNode.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            IMembersClassType classModel = new ClassModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpClassVisitor extractionVisitor)
                    {
                        classModel = extractionVisitor.Visit(baseTypeDeclarationSyntax, semanticModel, classModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Class Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.ClassTypes.Add(classModel);
        }

        return modelType;
    }
}
