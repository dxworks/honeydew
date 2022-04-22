using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class LocalVariablesTypeSetterVisitor : CompositeVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
    ICSharpMethodAccessorVisitor, ICSharpLocalFunctionVisitor, ICSharpDestructorVisitor
{
    public LocalVariablesTypeSetterVisitor(IEnumerable<ILocalVariablesVisitor> visitors) : base(visitors)
    {
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        SetLocalVariables(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        SetLocalVariables(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        SetLocalVariables(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IAccessorType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorType modelType)
    {
        SetLocalVariables(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
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
                                extractionVisitor.Visit(variableDeclaratorSyntax, semanticModel, localVariableModel);
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
                            extractionVisitor.Visit(declarationPatternSyntax, semanticModel, localVariableModel);
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
                            extractionVisitor.Visit(forEachStatementSyntax, semanticModel, localVariableModel);
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

    private void SetLocalVariables(SyntaxNode syntaxNode, SemanticModel semanticModel,
        ITypeWithLocalVariables typeWithLocalVariables)
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
                        localVariableModel =
                            extractionVisitor.Visit(variableDeclaratorSyntax, semanticModel, localVariableModel);
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
                        localVariableModel =
                            extractionVisitor.Visit(declarationPatternSyntax, semanticModel, localVariableModel);
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
                            extractionVisitor.Visit(forEachVariableStatementSyntax, semanticModel, localVariableModel);
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
