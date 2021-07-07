using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface IProjectLoadingStrategy
    {
        Task<ProjectModel> Load(Project project, IList<IFactExtractor> extractors);
    }
}