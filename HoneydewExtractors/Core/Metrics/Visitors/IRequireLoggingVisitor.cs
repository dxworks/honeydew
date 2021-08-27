using HoneydewCore.Logging;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IRequireLoggingVisitor : ITypeVisitor
    {
        public ILogger Logger { get; set; }
    }
}
