using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common
{
    public class LocalVariablesTypeSetterVisitor : CompositeVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
        ICSharpMethodAccessorVisitor
    {
        public LocalVariablesTypeSetterVisitor(IEnumerable<ILocalVariablesVisitor> visitors) : base(visitors)
        {
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            SetLocalVariables(syntaxNode, modelType);

            return modelType;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            SetLocalVariables(syntaxNode, modelType);

            return modelType;
        }

        public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            SetLocalVariables(syntaxNode, modelType);

            return modelType;
        }
        
        private void SetLocalVariables(SyntaxNode syntaxNode, ITypeWithLocalVariables typeWithLocalVariables)
        {
            foreach (var variableDeclaratorSyntax in
                syntaxNode.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                IEntityType localVariableModel = new EntityTypeModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpLocalVariablesVisitor extractionVisitor)
                        {
                            localVariableModel = extractionVisitor.Visit(variableDeclaratorSyntax, localVariableModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Local Variables Visitor because {e}", LogLevels.Warning);
                    }
                }

                typeWithLocalVariables.LocalVariableTypes.Add(localVariableModel);
            }
        }
    }
}
