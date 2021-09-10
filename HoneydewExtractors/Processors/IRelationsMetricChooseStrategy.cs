using System;

namespace HoneydewExtractors.Processors
{
    public interface IRelationsMetricChooseStrategy
    {
        bool Choose(Type type);
    }
}
