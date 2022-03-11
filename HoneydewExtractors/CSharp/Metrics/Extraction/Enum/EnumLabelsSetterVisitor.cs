using System;
using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Enum;

public class EnumLabelsSetterVisitor : CompositeVisitor, ICSharpEnumVisitor
{
    public EnumLabelsSetterVisitor(IEnumerable<IEnumLabelVisitor> visitors) : base(visitors)
    {
    }

    public IEnumType Visit(EnumDeclarationSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        foreach (var baseTypeDeclarationSyntax in syntaxNode.Members)
        {
            IEnumLabelType enumLabelType = new EnumLabelType();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpEnumLabelVisitor extractionVisitor)
                    {
                        enumLabelType =
                            extractionVisitor.Visit(baseTypeDeclarationSyntax, semanticModel, enumLabelType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Enum Label Visitor because {e}", LogLevels.Warning);
                }
            }

            modelType.Labels.Add(enumLabelType);
        }

        return modelType;
    }
}
