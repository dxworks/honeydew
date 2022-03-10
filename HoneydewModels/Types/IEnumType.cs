using System.Collections.Generic;

namespace HoneydewModels.Types;

public interface IEnumType : IClassType
{
    public string Type { get; set; }

    public IDictionary<string, int> Labels { get; set; }
}
