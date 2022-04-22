using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class GenericParameterSetterVisitor : CompositeVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
    ICSharpMethodVisitor, ICSharpLocalFunctionVisitor
{
    public GenericParameterSetterVisitor(ILogger logger, IEnumerable<IGenericParameterVisitor> visitors) : base(logger,
        visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    private void ExtractParameterInfo(SyntaxNode syntaxNode, SemanticModel semanticModel,
        ITypeWithGenericParameters modelType)
    {
        foreach (var typeParameterListSyntax in syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>())
        {
            foreach (var typeParameterSyntax in typeParameterListSyntax.Parameters)
            {
                IGenericParameterType parameterModel = new GenericParameterModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpGenericParameterVisitor extractionVisitor)
                        {
                            parameterModel =
                                extractionVisitor.Visit(typeParameterSyntax, semanticModel, parameterModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Generic Parameter Visitor because {e}",
                            LogLevels.Warning);
                    }
                }

                modelType.GenericParameters.Add(parameterModel);
            }
        }
    }
}
