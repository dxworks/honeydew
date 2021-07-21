using System.Threading.Tasks;
using HoneydewCore.Models;
using HoneydewModels;

namespace HoneydewCore.IO.Readers.ProjectRead
{
    public interface IProjectLoader
    {
        Task<ProjectModel> Load(string path);
    }
}
