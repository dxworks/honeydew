using System;
using System.Collections.Generic;
using Honeydew.Models;

namespace Honeydew.Extractors.Visitors.Setters;

public interface
    ISetterVisitor<in TSetterSyntaxNode, in TSemanticModel, TSetterType, TWrappedSyntaxNode, TWrappedType> :
        IExtractionVisitor<TSetterSyntaxNode, TSemanticModel, TSetterType>
{
    string Name();

    ILogger Logger { get; }

    IEnumerable<TWrappedSyntaxNode> GetWrappedSyntaxNodes(TSetterSyntaxNode syntaxNode);

    TWrappedType CreateWrappedType();

    IEnumerable<ITypeVisitor<TWrappedType>> GetContainedVisitors();

    TWrappedType ApplyContainedVisitors(TWrappedSyntaxNode wrappedSyntaxNode, TWrappedType wrappedType,
        TSemanticModel semanticModel)
    {
        foreach (var visitor in GetContainedVisitors())
        {
            try
            {
                if (visitor is IExtractionVisitor<TWrappedSyntaxNode, TSemanticModel, TWrappedType>
                    extractionVisitor)
                {
                    wrappedType = extractionVisitor.Visit(wrappedSyntaxNode, semanticModel, wrappedType);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Could not extract from {Name()} Visitor because {e}", LogLevels.Warning);
            }
        }

        return wrappedType;
    }
}
