using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class ConstructorSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public ConstructorSetterClassVisitor(IEnumerable<IConstructorVisitor> visitors) : base(visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var constructorDeclarationSyntax in syntaxNode.DescendantNodes()
                     .OfType<ConstructorDeclarationSyntax>())
        {
            IConstructorType constructorModel = new ConstructorModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpConstructorVisitor extractionVisitor)
                    {
                        constructorModel = extractionVisitor.Visit(constructorDeclarationSyntax, semanticModel,
                            constructorModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Constructor Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.Constructors.Add(constructorModel);
        }

        return modelType;
    }
}
