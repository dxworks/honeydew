using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class;

public class ConstructorSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
{
    public ConstructorSetterClassVisitor(IEnumerable<IConstructorVisitor> visitors) : base(visitors)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        if (modelType is not IMembersClassType membersClassType)
        {
            return modelType;
        }

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

            membersClassType.Constructors.Add(constructorModel);
        }

        return membersClassType;
    }
}
