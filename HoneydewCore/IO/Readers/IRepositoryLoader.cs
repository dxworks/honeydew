using System.Threading.Tasks;
using HoneydewCore.Models;
using HoneydewModels;

namespace HoneydewCore.IO.Readers
{
    public interface IRepositoryLoader
    {
        Task<RepositoryModel> Load(string path);
    }
}
