using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class LocalFunctionInfoVisitor : CompositeVisitor, ICSharpLocalFunctionVisitor
{
    public LocalFunctionInfoVisitor(IEnumerable<ILocalFunctionVisitor> visitors) : base(visitors)
    {
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        var returnType =
            GetFullName(syntaxNode.ReturnType, semanticModel, out var isNullable);
        var returnTypeModifier = SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            Modifier = returnTypeModifier,
            IsNullable = isNullable
        };
        modelType.Modifier = syntaxNode.Modifiers.ToString();

        modelType.AccessModifier = "";
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

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
                try
                {
                    if (visitor is ICSharpLocalFunctionVisitor extractionVisitor)
                    {
                        localFunction =
                            extractionVisitor.Visit(localFunctionStatementSyntax, semanticModel, localFunction);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Local Function Info Visitor because {e}", LogLevels.Warning);
                }
            }

            localFunction = Visit(localFunctionStatementSyntax, semanticModel, localFunction);

            modelType.LocalFunctions.Add(localFunction);
        }

        return modelType;
    }
}
