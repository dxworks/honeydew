using System.Collections.Generic;
using HoneydewModels;

namespace HoneydewExtractors
{
    public interface IFactExtractor<TClassModel>
        where TClassModel : IClassModel
    {
        public IList<TClassModel> Extract(string fileContent);
    }
}
