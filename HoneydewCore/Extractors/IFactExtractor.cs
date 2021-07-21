using System.Collections.Generic;
using HoneydewCore.Models;
using HoneydewModels;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Extractors
{
    public interface IFactExtractor
    {
        string FileType();

        IList<ClassModel> Extract(string fileContent);

        IList<ClassModel> Extract(SyntaxTree fileContent);
    }
}
