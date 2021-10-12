using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common
{
    public class LocalVariablesTypeSetterVisitor : CompositeVisitor, IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpMethodVisitor, ICSharpConstructorVisitor,
        ICSharpMethodAccessorVisitor, ICSharpLocalFunctionVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

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

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            var missingLocalVariablesCount = 0;

            // normal local variables
            foreach (var localDeclarationStatementSyntax in syntaxNode.Body.ChildNodes()
                .OfType<LocalDeclarationStatementSyntax>())
            {
                foreach (var variableDeclaratorSyntax in localDeclarationStatementSyntax.DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>())
                {
                    ILocalVariableType localVariableModel = new LocalVariableModel();

                    foreach (var visitor in GetContainedVisitors())
                    {
                        try
                        {
                            if (visitor is ICSharpLocalVariablesVisitor extractionVisitor)
                            {
                                localVariableModel =
                                    extractionVisitor.Visit(variableDeclaratorSyntax, localVariableModel);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Could not extract from Local Function Local Variables Visitor because {e}",
                                LogLevels.Warning);
                        }
                    }

                    if (!string.IsNullOrEmpty(localVariableModel.Type.Name) &&
                        localVariableModel.Type.Name != CSharpConstants.VarIdentifier)
                    {
                        modelType.LocalVariableTypes.Add(localVariableModel);
                    }
                    else
                    {
                        missingLocalVariablesCount++;
                    }
                }
            }

            // local variables from ifs and switches
            foreach (var declarationPatternSyntax in syntaxNode.Body.DescendantNodes()
                .OfType<DeclarationPatternSyntax>())
            {
                var parentDeclarationSyntax =
                    declarationPatternSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>();
                if (parentDeclarationSyntax != syntaxNode)
                {
                    continue;
                }

                ILocalVariableType localVariableModel = new LocalVariableModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpLocalVariablesVisitor extractionVisitor)
                        {
                            localVariableModel =
                                extractionVisitor.Visit(declarationPatternSyntax, localVariableModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(
                            $"Could not extract from Local Function Ifs and Switches Local Variables Visitor because {e}",
                            LogLevels.Warning);
                    }
                }

                if (!string.IsNullOrEmpty(localVariableModel.Type.Name) &&
                    localVariableModel.Type.Name != CSharpConstants.VarIdentifier)
                {
                    modelType.LocalVariableTypes.Add(localVariableModel);
                }
                else
                {
                    missingLocalVariablesCount++;
                }
            }

            // local variables from foreach
            foreach (var forEachStatementSyntax in syntaxNode.Body.ChildNodes()
                .OfType<ForEachStatementSyntax>())
            {
                ILocalVariableType localVariableModel = new LocalVariableModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpLocalVariablesVisitor extractionVisitor)
                        {
                            localVariableModel =
                                extractionVisitor.Visit(forEachStatementSyntax, localVariableModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Local Function Foreach Local Variables Visitor because {e}",
                            LogLevels.Warning);
                    }
                }

                if (!string.IsNullOrEmpty(localVariableModel.Type.Name) &&
                    localVariableModel.Type.Name != CSharpConstants.VarIdentifier)
                {
                    modelType.LocalVariableTypes.Add(localVariableModel);
                }
                else
                {
                    missingLocalVariablesCount++;
                }
            }

            if (missingLocalVariablesCount > 0)
            {
                Logger.Log($"Could not set {missingLocalVariablesCount} local variables", LogLevels.Warning);
            }

            return modelType;
        }

        private void SetLocalVariables(SyntaxNode syntaxNode, ITypeWithLocalVariables typeWithLocalVariables)
        {
            var missingLocalVariablesCount = 0;

            // normal local variables
            foreach (var variableDeclaratorSyntax in
                syntaxNode.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                ILocalVariableType localVariableModel = new LocalVariableModel();

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

                if (!string.IsNullOrEmpty(localVariableModel.Type.Name) &&
                    localVariableModel.Type.Name != CSharpConstants.VarIdentifier)
                {
                    typeWithLocalVariables.LocalVariableTypes.Add(localVariableModel);
                }
                else
                {
                    missingLocalVariablesCount++;
                }
            }

            // local variables from ifs and switches
            foreach (var declarationPatternSyntax in
                syntaxNode.DescendantNodes().OfType<DeclarationPatternSyntax>())
            {
                ILocalVariableType localVariableModel = new LocalVariableModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpLocalVariablesVisitor extractionVisitor)
                        {
                            localVariableModel = extractionVisitor.Visit(declarationPatternSyntax, localVariableModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Ifs and Switches Local Variables Visitor because {e}",
                            LogLevels.Warning);
                    }
                }

                if (!string.IsNullOrEmpty(localVariableModel.Type.Name) &&
                    localVariableModel.Type.Name != CSharpConstants.VarIdentifier)
                {
                    typeWithLocalVariables.LocalVariableTypes.Add(localVariableModel);
                }
                else
                {
                    missingLocalVariablesCount++;
                }
            }

            // local variables from foreach
            foreach (var forEachVariableStatementSyntax in
                syntaxNode.DescendantNodes().OfType<ForEachStatementSyntax>())
            {
                ILocalVariableType localVariableModel = new LocalVariableModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpLocalVariablesVisitor extractionVisitor)
                        {
                            localVariableModel =
                                extractionVisitor.Visit(forEachVariableStatementSyntax, localVariableModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Foreach Local Variables Visitor because {e}",
                            LogLevels.Warning);
                    }
                }

                if (!string.IsNullOrEmpty(localVariableModel.Type.Name) &&
                    localVariableModel.Type.Name != CSharpConstants.VarIdentifier)
                {
                    typeWithLocalVariables.LocalVariableTypes.Add(localVariableModel);
                }
                else
                {
                    missingLocalVariablesCount++;
                }
            }

            if (missingLocalVariablesCount > 0)
            {
                Logger.Log($"Could not set {missingLocalVariablesCount} local variables", LogLevels.Warning);
            }
        }
    }
}
