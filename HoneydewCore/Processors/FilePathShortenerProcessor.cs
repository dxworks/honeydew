using System.IO;
using System.Linq;
using Honeydew.Models;
using HoneydewCore.IO;

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
            _inputFilePath = inputFilePath;

            if (!folderPathValidator.IsFolder(inputFilePath))
            {
                var directoryInfo = Directory.GetParent(_inputFilePath);
                if (directoryInfo != null)
                {
                    _inputFilePath = directoryInfo.ToString();
                }
            }

            _inputFilePath = _inputFilePath.Replace('\\', '/');
        }

        public RepositoryModel Process(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                if (!string.IsNullOrWhiteSpace(solutionModel.FilePath))
                {
                    solutionModel.FilePath = TrimPath(solutionModel.FilePath);
                }

                solutionModel.ProjectsPaths = solutionModel.ProjectsPaths
                    .Select(path => !string.IsNullOrWhiteSpace(path) ? TrimPath(path) : path).ToList();
            }

            foreach (var projectModel in repositoryModel.Projects)
            {
                if (!string.IsNullOrWhiteSpace(projectModel.FilePath))
                {
                    projectModel.FilePath = TrimPath(projectModel.FilePath);
                }

                for (var i = 0; i < projectModel.ProjectReferences.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(projectModel.ProjectReferences[i]))
                    {
                        projectModel.ProjectReferences[i] = TrimPath(projectModel.ProjectReferences[i]);
                    }
                }

                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    if (!string.IsNullOrWhiteSpace(compilationUnitType.FilePath))
                    {
                        compilationUnitType.FilePath = TrimPath(compilationUnitType.FilePath);
                    }

                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        if (!string.IsNullOrWhiteSpace(classType.FilePath))
                        {
                            classType.FilePath = TrimPath(classType.FilePath);
                        }
                    }
                }
            }

            return repositoryModel;
        }

        private string TrimPath(string path)
        {
            if (!AreSubDirectories(_inputFilePath, path))
            {
                return path.Replace('\\', '/');
            }

            var relativePath = Path.GetRelativePath(_inputFilePath, path);

            return relativePath.Replace('\\', '/');
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
