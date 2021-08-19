using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class CompilationUnitModel : ICompilationUnitType
    {
        public IList<IClassType> ClassTypes { get; set; } = new List<IClassType>();

        public string FilePath { get; set; }

        public IList<IImportType> Imports { get; set; } = new List<IImportType>();
    }
}
