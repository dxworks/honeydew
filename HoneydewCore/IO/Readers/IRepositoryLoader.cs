using System.Threading.Tasks;
using HoneydewModels;

namespace HoneydewCore.IO.Readers
{
    public interface IRepositoryLoader<TRepositoryModel>
        where TRepositoryModel : IRepositoryModel
    {
        Task<TRepositoryModel> Load(string path);
    }
}
