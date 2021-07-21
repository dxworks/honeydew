﻿using System.Collections.Generic;

namespace HoneydewModels.Representations.ReferenceModel
{
    public record ReferenceProjectModel : ReferenceEntity
    {
        public ReferenceSolutionModel SolutionReference { get; init; }

        public IList<ReferenceNamespaceModel> Namespaces { get; } = new List<ReferenceNamespaceModel>();
    }
}
