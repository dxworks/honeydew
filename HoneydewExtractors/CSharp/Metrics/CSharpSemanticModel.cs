using HoneydewExtractors.Core.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSemanticModel : ISemanticModel
    {
        public SemanticModel Model { get; init; }
    }
}
