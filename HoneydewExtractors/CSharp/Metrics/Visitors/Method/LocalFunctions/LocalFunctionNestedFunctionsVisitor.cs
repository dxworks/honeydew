using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions
{
    public class LocalFunctionNestedFunctionsVisitor : CompositeVisitor,
        ICSharpLocalFunctionVisitor
    {
        public LocalFunctionNestedFunctionsVisitor(IEnumerable<ILocalFunctionVisitor> visitors) : base(visitors)
        {
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            foreach (var localFunctionStatementSyntax in syntaxNode.Body.ChildNodes()
                .OfType<LocalFunctionStatementSyntax>())
            {
                IMethodTypeWithLocalFunctions localFunction = new MethodModel();
                foreach (var visitor in GetContainedVisitors())
                {
                    if (visitor is ICSharpLocalFunctionVisitor extractionVisitor)
                    {
                        localFunction = extractionVisitor.Visit(localFunctionStatementSyntax, localFunction);
                    }
                }

                localFunction = Visit(localFunctionStatementSyntax, localFunction);

                modelType.LocalFunctions.Add(localFunction);
            }

            return modelType;
        }
    }
}
