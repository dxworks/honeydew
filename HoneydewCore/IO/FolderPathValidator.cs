using System.IO;

namespace HoneydewCore.IO
{
    public class FolderPathValidator : IFolderPathValidator
    {
        public bool IsFolder(string path)
        {
            return Directory.Exists(path);
        }
    }
}
