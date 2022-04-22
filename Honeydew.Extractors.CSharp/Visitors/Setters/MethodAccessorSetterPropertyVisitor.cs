using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class MethodAccessorSetterPropertyVisitor : CompositeVisitor, ICSharpPropertyVisitor
{
    public MethodAccessorSetterPropertyVisitor(IEnumerable<IMethodVisitor> visitors) : base(visitors)
    {
    }

    public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        // check if property has expression-bodied member
        if (syntaxNode.AccessorList == null)
        {
            if (syntaxNode is not PropertyDeclarationSyntax { ExpressionBody: { } } propertyDeclarationSyntax)
            {
                return modelType;
            }

            IAccessorType accessorModel = new AccessorModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpArrowExpressionMethodVisitor extractionVisitor)
                    {
                        accessorModel = extractionVisitor.Visit(propertyDeclarationSyntax.ExpressionBody,
                            semanticModel, accessorModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Arrow Method Accessor Visitor because {e}",
                        LogLevels.Warning);
                }
            }

            modelType.Accessors.Add(accessorModel);

            return modelType;
        }

        // property has accessors
        foreach (var accessor in syntaxNode.AccessorList.Accessors)
        {
            IAccessorType accessorModel = new AccessorModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpMethodAccessorVisitor extractionVisitor)
                    {
                        accessorModel = extractionVisitor.Visit(accessor, semanticModel, accessorModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Method Accessor Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.Accessors.Add(accessorModel);
        }

        return modelType;
    }
}
