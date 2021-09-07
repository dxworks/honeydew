using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Parameters
{
    public interface ICSharpGenericParameterVisitor : IGenericParameterVisitor,
        IExtractionVisitor<TypeParameterSyntax, IGenericParameterType>
    {
    }
}
