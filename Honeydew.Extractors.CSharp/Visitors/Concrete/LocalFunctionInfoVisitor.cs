using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class LocalFunctionInfoVisitor : CompositeVisitor<IMethodTypeWithLocalFunctions>,
    IExtractionVisitor<LocalFunctionStatementSyntax, SemanticModel, IMethodTypeWithLocalFunctions>
{
    public LocalFunctionInfoVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IMethodTypeWithLocalFunctions>> visitors) : base(compositeLogger, visitors)
    {
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        modelType.Name = syntaxNode.Identifier.ToString();
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
                    if (visitor is IExtractionVisitor<LocalFunctionStatementSyntax, SemanticModel,
                            IMethodTypeWithLocalFunctions> extractionVisitor)
                    {
                        localFunction =
                            extractionVisitor.Visit(localFunctionStatementSyntax, semanticModel, localFunction);
                    }
                }
                catch (Exception e)
                {
                    CompositeLogger.Log($"Could not extract from Local Function Info Visitor because {e}",
                        LogLevels.Warning);
                }
            }

            localFunction = Visit(localFunctionStatementSyntax, semanticModel, localFunction);

            modelType.LocalFunctions.Add(localFunction);
        }

        return modelType;
    }
}
