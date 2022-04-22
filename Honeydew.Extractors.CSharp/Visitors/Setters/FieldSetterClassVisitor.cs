using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class FieldSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public FieldSetterClassVisitor(ILogger logger, IEnumerable<IFieldVisitor> visitors) : base(logger, visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var baseFieldDeclarationSyntax in
                 syntaxNode.DescendantNodes().OfType<BaseFieldDeclarationSyntax>())
        {
            IList<IFieldType> fieldTypes = new List<IFieldType>();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpFieldVisitor extractionVisitor)
                    {
                        fieldTypes = extractionVisitor.Visit(baseFieldDeclarationSyntax, semanticModel, fieldTypes);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Field Visitor because {e}", LogLevels.Warning);
                }
            }

            foreach (var fieldType in fieldTypes)
            {
                modelType.Fields.Add(fieldType);
            }
        }

        return modelType;
    }
}
