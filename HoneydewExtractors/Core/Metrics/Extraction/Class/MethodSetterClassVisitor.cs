using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class MethodSetterClassVisitor : CompositeVisitor, ICSharpClassVisitor
    {
        public MethodSetterClassVisitor(IEnumerable<IMethodVisitor> visitors): base(visitors)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return modelType;
            }

            foreach (var methodDeclarationSyntax in syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                IMethodType methodModel = new MethodModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    if (visitor is ICSharpMethodVisitor extractionVisitor)
                    {
                        methodModel = extractionVisitor.Visit(methodDeclarationSyntax, methodModel);
                    }
                }

                membersClassType.Methods.Add(methodModel);
            }

            return membersClassType;
        }
    }
}
