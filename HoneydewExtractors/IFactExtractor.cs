using System.Collections.Generic;
using HoneydewModels;

namespace HoneydewExtractors
{
    public interface IFactExtractor
    {
        public IList<IClassModel> Extract(string fileContent);
    }
}
