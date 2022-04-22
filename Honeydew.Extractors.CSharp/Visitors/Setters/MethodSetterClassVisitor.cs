using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class MethodSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public MethodSetterClassVisitor(ILogger logger, IEnumerable<IMethodVisitor> visitors) : base(logger, visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var methodDeclarationSyntax in syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            IMethodType methodModel = new MethodModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpMethodVisitor extractionVisitor)
                    {
                        methodModel = extractionVisitor.Visit(methodDeclarationSyntax, semanticModel, methodModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Method Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.Methods.Add(methodModel);
        }

        return modelType;
    }
}
