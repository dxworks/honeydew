using System.Collections.Generic;
using HoneydewCore.Models;

namespace HoneydewCore.Extractors
{
    public interface IFactExtractor
    {
        string FileType();

        IList<ProjectClassModel> Extract(string fileContent);
    }
}