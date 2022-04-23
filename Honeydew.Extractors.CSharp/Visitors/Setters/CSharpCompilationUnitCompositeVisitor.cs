using Honeydew.Extractors.Visitors;
using Honeydew.Models;
using Honeydew.Models.Types;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpCompilationUnitCompositeVisitor : CompositeVisitor<ICompilationUnitType>
{
    public CSharpCompilationUnitCompositeVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<ICompilationUnitType>> visitors) : base(compositeLogger, visitors)
    {
    }
}
