using System.Threading.Tasks;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public interface IRepositoryLoader
    {
        Task<RepositoryModel> Load(string path);
    }
}