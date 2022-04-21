using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ICompilationUnitType : ITypeWithLinesOfCode, ITypeWithMetrics
{
    public IList<IClassType> ClassTypes { get; set; }

    public string FilePath { get; set; }

    public IList<IImportType> Imports { get; set; }
}
