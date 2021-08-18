﻿using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record MethodModel : IModelEntity, IMethodType
    {
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public string Modifier { get; init; } = "";

        public string AccessModifier { get; init; }
        public IReturnType ReturnType { get; set; }

        public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();
        public IList<IMethodSignatureType> CalledMethods { get; set; } = new List<IMethodSignatureType>();

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public int CyclomaticComplexity { get; set; }

        public LinesOfCode Loc { get; set; }
    }
}
