using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicCompilationUnitCompositeVisitor: CompositeVisitor<ICompilationUnitType>
{
    public VisualBasicCompilationUnitCompositeVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<ICompilationUnitType>> visitors) : base(compositeLogger, visitors)
    {
    }
}
