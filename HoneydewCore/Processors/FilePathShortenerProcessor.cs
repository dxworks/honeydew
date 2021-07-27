using System.IO;
using HoneydewCore.IO;
using HoneydewModels.CSharp;

namespace HoneydewCore.Processors
{
    public class FilePathShortenerProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        private readonly string _inputFilePath;

        public FilePathShortenerProcessor(string inputFilePath) : this(new FolderPathValidator(), inputFilePath)
        {
        }

        public FilePathShortenerProcessor(IFolderPathValidator folderPathValidator, string inputFilePath)
        {
            inputFilePath = inputFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            _inputFilePath = inputFilePath;

            if (!folderPathValidator.IsFolder(inputFilePath))
            {
                var directoryInfo = Directory.GetParent(_inputFilePath);
                if (directoryInfo != null)
                {
                    _inputFilePath = directoryInfo.ToString();
                }
            }
        }

        public RepositoryModel Process(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                if (!string.IsNullOrWhiteSpace(solutionModel.FilePath))
                {
                    solutionModel.FilePath = TrimPath(solutionModel.FilePath);
                }

                foreach (var projectModel in solutionModel.Projects)
                {
                    if (!string.IsNullOrWhiteSpace(projectModel.FilePath))
                    {
                        projectModel.FilePath = TrimPath(projectModel.FilePath);
                    }

                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            if (!string.IsNullOrWhiteSpace(classModel.FilePath))
                            {
                                classModel.FilePath = TrimPath(classModel.FilePath);
                            }
                        }
                    }
                }
            }

            return repositoryModel;
        }

        private string TrimPath(string path)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            if (!AreSubDirectories(_inputFilePath, path))
            {
                return path;
            }

            return Path.GetRelativePath(_inputFilePath, path);
        }

        private static bool AreSubDirectories(string path1, string path2)
        {
            var di1 = new DirectoryInfo(path1);
            var di2 = new DirectoryInfo(path2);
            var isParent = false;
            while (di2.Parent != null)
            {
                if (di2.Parent.FullName == di1.FullName)
                {
                    isParent = true;
                    break;
                }

                di2 = di2.Parent;
            }

            return isParent;
        }
    }
}
