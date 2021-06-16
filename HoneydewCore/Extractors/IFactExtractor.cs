using System.Collections.Generic;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Extractors
{
    public interface IFactExtractor
    {
        string FileType();

        IList<ProjectClassModel> Extract(string fileContent);

        IList<ProjectClassModel> Extract(SyntaxTree fileContent);
    }
}