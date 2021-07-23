﻿using System.Collections.Generic;
using HoneydewModels.Representations;

namespace HoneydewCore.Extractors.Metrics
{
    public interface IRelationMetric
    {
        IList<FileRelation> GetRelations(object metricValue);
    }
}
