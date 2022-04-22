using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class PropertySetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public PropertySetterClassVisitor(IEnumerable<IPropertyVisitor> visitors) : base(visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMembersClassType modelType)
    {
        if (modelType is not IPropertyMembersClassType propertyMembersClassType)
        {
            return modelType;
        }

        foreach (var basePropertyDeclarationSyntax in syntaxNode.DescendantNodes()
                     .OfType<BasePropertyDeclarationSyntax>())
        {
            IPropertyType propertyModel = new PropertyModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpPropertyVisitor extractionVisitor)
                    {
                        propertyModel =
                            extractionVisitor.Visit(basePropertyDeclarationSyntax, semanticModel, propertyModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Property Visitor because {e}", LogLevels.Warning);
                }
            }

            propertyMembersClassType.Properties.Add(propertyModel);
        }

        return propertyMembersClassType;
    }
}
