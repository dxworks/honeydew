using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class;

public class FieldSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public FieldSetterClassVisitor(IEnumerable<IFieldVisitor> visitors) : base(visitors)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        if (modelType is not IMembersClassType membersClassType)
        {
            return modelType;
        }

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
                membersClassType.Fields.Add(fieldType);
            }
        }

        return membersClassType;
    }
}
