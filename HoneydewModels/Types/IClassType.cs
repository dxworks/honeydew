﻿using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IClassType : IContainedType, IModifierType, ITypeWithAttributes, ITypeWithMetrics
    {
        public string ClassType { get; init; }

        public string FilePath { get; set; }

        public IList<IBaseType> BaseTypes { get; set; }

        public IList<IImportType> Imports { get; set; }
    }
}
