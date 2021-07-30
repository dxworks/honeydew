using System.Collections.Generic;
using HoneydewModels;

namespace HoneydewExtractors.Core
{
    public interface IFactExtractor<TClassModel>
        where TClassModel : IClassModel
    {
        public IList<TClassModel> Extract(string fileContent);
    }
}
