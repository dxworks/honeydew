﻿using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record ParameterModel : IParameterType
    {
        public IEntityType Type { get; set; }

        public string Modifier { get; set; } = "";

        public string DefaultValue { get; set; }

        public bool IsNullable { get; set; }

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}
