using System.Collections.Generic;
using HoneydewModels;

namespace HoneydewExtractors
{
    public interface IClassModelExtractor
    {
        IList<IClassModel> Extract(ISyntacticModel syntacticModel, ISemanticModel semanticModel);
    }
}
