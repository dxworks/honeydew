using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common
{
    public class AccessedFieldsSetterVisitor : CompositeVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
        ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor, ICSharpArrowExpressionMethodVisitor,
        ICSharpDestructorVisitor
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

        public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, IDestructorType modelType)
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
            IMethodTypeWithLocalFunctions containedTypeWithAccessedFields)
        {
            if (syntaxNode.Body == null)
            {
                return containedTypeWithAccessedFields;
            }

            var descendantNodes = syntaxNode.Body.ChildNodes().ToList();
            var possibleMemberAccessExpressions = descendantNodes.OfType<LocalDeclarationStatementSyntax>()
                .SelectMany(syntax => GetPossibleAccessFields(syntax.DescendantNodes().ToList()));

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Concat(descendantNodes
                .OfType<ExpressionStatementSyntax>()
                .SelectMany(syntax => GetPossibleAccessFields(syntax.DescendantNodes().ToList())));

            foreach (var memberAccessExpressionSyntax in possibleMemberAccessExpressions)
            {
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
                        Logger.Log($"Could not extract from Local Function Accessed Fields Visitor because {e}",
                            LogLevels.Warning);
                    }
                }

                if (shouldIgnoreIdentifierBecauseIsNotField)
                {
                    continue;
                }

                containedTypeWithAccessedFields.AccessedFields.Add(accessedField);
            }

            return containedTypeWithAccessedFields;
        }

        private void SetAccessedFields(SyntaxNode syntaxNode,
            IContainedTypeWithAccessedFields containedTypeWithAccessedFields)
        {
            var descendantNodes = syntaxNode.DescendantNodes().ToList();
            var possibleMemberAccessExpressions = GetPossibleAccessFields(descendantNodes);

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

        private static List<ExpressionSyntax> GetPossibleAccessFields(List<SyntaxNode> descendantNodes)
        {
            var possibleMemberAccessExpressions = descendantNodes
                .OfType<MemberAccessExpressionSyntax>().Cast<ExpressionSyntax>().ToList();

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Concat(descendantNodes
                .OfType<VariableDeclaratorSyntax>()
                .Select(syntax => syntax.Initializer?.Value)).ToList();

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Concat(descendantNodes
                .OfType<AssignmentExpressionSyntax>()
                .SelectMany(syntax => new List<ExpressionSyntax> { syntax.Left, syntax.Right })).ToList();

            possibleMemberAccessExpressions = possibleMemberAccessExpressions.Distinct().ToList();
            return possibleMemberAccessExpressions;
        }
    }
}
