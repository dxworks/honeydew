using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.ProjectRead
{
    public interface IProjectProvider
    {
        Task<Project> GetProject(string path);
    }
}