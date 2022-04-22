using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class DestructorSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public DestructorSetterClassVisitor(ILogger logger, IEnumerable<IDestructorVisitor> visitors) : base(logger,
        visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var destructorDeclarationSyntax in syntaxNode.DescendantNodes()
                     .OfType<DestructorDeclarationSyntax>())
        {
            IDestructorType destructorType = new DestructorModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpDestructorVisitor extractionVisitor)
                    {
                        destructorType = extractionVisitor.Visit(destructorDeclarationSyntax, semanticModel,
                            destructorType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Destructor Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.Destructor = destructorType;
        }

        return modelType;
    }
}
