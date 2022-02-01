using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class
{
    public class DestructorSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
    {
        public DestructorSetterClassVisitor(IEnumerable<IDestructorVisitor> visitors) : base(visitors)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return modelType;
            }

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
                            destructorType = extractionVisitor.Visit(destructorDeclarationSyntax, destructorType);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Destructor Visitor because {e}", LogLevels.Warning);
                    }
                }

                membersClassType.Destructor = destructorType;
            }

            return membersClassType;
        }
    }
}
