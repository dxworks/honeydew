using System.IO;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    internal static class ActualFilePathProvider
    {
        public static string GetActualFilePath(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return GetProperFilePathCapitalization(filePath);
                }

                if (Directory.Exists(filePath))
                {
                    return GetProperDirectoryCapitalization(new DirectoryInfo(filePath));
                }

                return filePath;
            }
            catch
            {
                return filePath;
            }
        }

        private static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            if (null == parentDirInfo)
            {
                return dirInfo.Root.FullName;
            }

            return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        private static string GetProperFilePathCapitalization(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var dirInfo = fileInfo.Directory;

            return Path.Combine(GetProperDirectoryCapitalization(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }
    }
}
