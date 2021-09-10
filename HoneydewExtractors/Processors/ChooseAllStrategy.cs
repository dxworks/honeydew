using System;

namespace HoneydewExtractors.Processors
{
    public class ChooseAllStrategy : IRelationsMetricChooseStrategy
    {
        public bool Choose(Type type)
        {
            return true;
        }
    }
}
