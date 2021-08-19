using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface ICompilationUnitType
    {
        public IList<IClassType> ClassTypes { get; set; }

        public string FilePath { get; set; }

        public IList<IImportType> Imports { get; set; }
    }
}
