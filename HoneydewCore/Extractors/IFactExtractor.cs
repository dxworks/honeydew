using System.Collections.Generic;
using HoneydewCore.Extractors.Models;

namespace HoneydewCore.Extractors
{
    public interface IFactExtractor
    {
        string FileType();

        IList<ClassModel> Extract(string fileContent);
    }
}