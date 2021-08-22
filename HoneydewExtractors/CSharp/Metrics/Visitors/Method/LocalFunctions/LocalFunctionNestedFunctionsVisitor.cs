using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions
{
    public class LocalFunctionNestedFunctionsVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpLocalFunctionVisitor
    {
        private readonly IList<ICSharpLocalFunctionVisitor> _localFunctionVisitor;

        private bool _inheritedModelsSet;

        public LocalFunctionNestedFunctionsVisitor(IList<ICSharpLocalFunctionVisitor> localFunctionVisitor)
        {
            _localFunctionVisitor = localFunctionVisitor;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            if (!_inheritedModelsSet)
            {
                _inheritedModelsSet = true;
                foreach (var visitor in _localFunctionVisitor)
                {
                    if (visitor is not ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>
                        extractionVisitor)
                    {
                        continue;
                    }

                    extractionVisitor.SetSyntacticModel(InheritedSyntacticModel);
                    extractionVisitor.SetSemanticModel(InheritedSemanticModel);
                }
            }

            foreach (var localFunctionStatementSyntax in syntaxNode.Body.ChildNodes()
                .OfType<LocalFunctionStatementSyntax>())
            {
                IMethodTypeWithLocalFunctions localFunction = new MethodModel();
                foreach (var visitor in _localFunctionVisitor)
                {
                    localFunction = visitor.Visit(localFunctionStatementSyntax, localFunction);
                }

                localFunction = Visit(localFunctionStatementSyntax, localFunction);

                modelType.LocalFunctions.Add(localFunction);
            }

            return modelType;
        }
    }
}
