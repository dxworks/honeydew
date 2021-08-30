﻿using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class PropertySetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
    {
        public PropertySetterClassVisitor(IEnumerable<IPropertyVisitor> visitors) : base(visitors)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
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
                            propertyModel = extractionVisitor.Visit(basePropertyDeclarationSyntax, propertyModel);
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
}
