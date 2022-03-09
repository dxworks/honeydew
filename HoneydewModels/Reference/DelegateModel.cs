using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class DelegateModel : EntityModel
{
    public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

    public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

    public ReturnValueModel ReturnValue { get; set; }
}
