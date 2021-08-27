using HoneydewCore.Logging;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public class LoggerSetterVisitor : IVisitor
    {
        private readonly ILogger _logger;

        public LoggerSetterVisitor(ILogger logger)
        {
            _logger = logger;
        }

        public void Visit(ITypeVisitor visitor)
        {
            if (visitor is IRequireLoggingVisitor requireLoggingVisitor)
            {
                requireLoggingVisitor.Logger = _logger;
            }

            visitor.Accept(this);
        }
    }
}
