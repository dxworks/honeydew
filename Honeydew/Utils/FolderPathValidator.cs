using System.IO;

namespace Honeydew.Utils;

public class FolderPathValidator : IFolderPathValidator
{
    public bool IsFolder(string path)
    {
        return Directory.Exists(path);
    }
}
