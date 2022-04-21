using System.Collections.Generic;

namespace HoneydewModels.Types;

public interface IEnumType : IClassType
{
    public string Type { get; set; }

    public IList<IEnumLabelType> Labels { get; set; }
}
