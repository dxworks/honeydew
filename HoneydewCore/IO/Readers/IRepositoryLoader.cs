using System.Threading.Tasks;

namespace HoneydewCore.IO.Readers
{
    public interface IRepositoryLoader<TRepositoryModel>
    {
        Task<TRepositoryModel> Load(string path);
    }
}
