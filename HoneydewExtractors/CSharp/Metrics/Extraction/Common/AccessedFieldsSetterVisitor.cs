using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common
{
    public class AccessedFieldsSetterVisitor : CompositeVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor,
        ICSharpArrowExpressionMethodVisitor
    {
        public AccessedFieldsSetterVisitor(IEnumerable<IAccessedFieldsVisitor> visitors) : base(visitors)
        {
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            SetAccessedFields(syntaxNode, modelType);

            return modelType;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            SetAccessedFields(syntaxNode, modelType);

            return modelType;
        }

        public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            SetAccessedFields(syntaxNode, modelType);

            return modelType;
        }

        public IMethodType Visit(ArrowExpressionClauseSyntax syntaxNode, IMethodType modelType)
        {
            SetAccessedFields(syntaxNode, modelType);

            return modelType;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            throw new NotImplementedException();
        }

        private void SetAccessedFields(SyntaxNode syntaxNode,
            IContainedTypeWithAccessedFields containedTypeWithAccessedFields)
        {
            var descendantNodes = syntaxNode.DescendantNodes().ToList();
            var possibleMemberAccessExpressions = descendantNodes
                .OfType<MemberAccessExpressionSyntax>().Cast<ExpressionSyntax>().ToList();

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Concat(descendantNodes
                .OfType<VariableDeclaratorSyntax>()
                .Select(syntax => syntax.Initializer?.Value)).ToList();

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Concat(descendantNodes
                .OfType<AssignmentExpressionSyntax>()
                .SelectMany(syntax => new List<ExpressionSyntax> { syntax.Left, syntax.Right })).ToList();

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Distinct().ToList();
            
            foreach (var memberAccessExpressionSyntax in possibleMemberAccessExpressions)
            {
                if (memberAccessExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() != null)
                {
                    continue;
                }

                var accessedField = new AccessedField();

                var shouldIgnoreIdentifierBecauseIsNotField = false;
                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpAccessedFieldsVisitor extractionVisitor)
                        {
                            accessedField = extractionVisitor.Visit(memberAccessExpressionSyntax, accessedField);
                            if (accessedField == null)
                            {
                                shouldIgnoreIdentifierBecauseIsNotField = true;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Accessed Field Visitor because {e}", LogLevels.Warning);
                    }
                }

                if (shouldIgnoreIdentifierBecauseIsNotField)
                {
                    continue;
                }

                containedTypeWithAccessedFields.AccessedFields.Add(accessedField);
            }
        }
    }
}
