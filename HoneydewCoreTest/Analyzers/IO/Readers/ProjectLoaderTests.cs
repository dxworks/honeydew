using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Analyzers.IO.Readers
{
    public class ProjectLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<IFileReader> _fileReaderMock = new();

        public ProjectLoaderTests()
        {
            _sut = new SolutionLoader(_fileReaderMock.Object);
        }

        [Fact]
        public void LoadProjectTest_ShouldThrowProjectNotFoundException_WhenGivenAnInvalidPath()
        {
            string pathToProject = "invalidPathToProject";
            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(new List<string>());

            var fileNotFoundException =
                Assert.Throws<ProjectNotFoundException>(() => _sut.LoadSolution(pathToProject));
            Assert.Equal("Project not found at specified Path", fileNotFoundException.Message);
        }

        [Fact]
        public void LoadProjectTest_ShouldReadAllFilesFromFolder_WhenGivenAValidPathToAProject()
        {
            string pathToProject = "validPathToProject";
            var pathsList = new List<string>
            {
                "validPathToProject/file1.cs",
                "validPathToProject/file2.cs",
                "validPathToProject/file3.txt",
                "validPathToProject/folder/f1.cs",
                "validPathToProject/folder/f2.cs",
                "validPathToProject/res/f3.cs",
                "validPathToProject/res/images/img.png",
            };

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(pathsList);

            foreach (string path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            var projectModel = _sut.LoadSolution(pathToProject);

            Assert.NotNull(projectModel);

            foreach (string path in pathsList)
            {
                _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
            }
        }

        [Fact]
        public void LoadProjectTest_ShouldReadAllCSFilesFromFolder_WhenGivenValidPathToAProject_AndOneFilter()
        {
            string pathToProject = "validPathToProject";
            var pathsList = new List<string>
            {
                $"{pathToProject}/file1.cs",
                $"{pathToProject}/file2.xml",
                $"{pathToProject}/file.xaml",
                $"{pathToProject}/file.xaml.cs",
                $"{pathToProject}/.gitignore",
                $"{pathToProject}/.vs",
                $"{pathToProject}/folder/f1.cs",
                $"{pathToProject}/folder/f2.cs",
                $"{pathToProject}/res/f3.cs",
                $"{pathToProject}/res/images/img.png",
            };

            IList<PathFilter> filters = new List<PathFilter>();
            filters.Add(path => path.EndsWith(".cs"));

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(pathsList);

            foreach (string path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            var projectModel = _sut.LoadSolution(pathToProject, filters);

            Assert.NotNull(projectModel);

            foreach (string path in pathsList)
            {
                if (path.EndsWith(".cs"))
                {
                    _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
                }
                else
                {
                    _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Never);
                }
            }
        }

        [Theory]
        [InlineData(new object[] {new[] {".cs", ".xml", ".xaml",}})]
        [InlineData(new object[] {new[] {".vs", ".gitignore"}})]
        public void LoadProjectTest_ShouldReadAllFilteredFilesWithExtensionFromFolder_WhenGivenValidPathToAProject(
            string[] fileExtensions)
        {
            string pathToProject = "validPathToProject";
            var pathsList = new List<string>
            {
                $"{pathToProject}/file1.cs",
                $"{pathToProject}/file2.xml",
                $"{pathToProject}/file.xaml",
                $"{pathToProject}/file.xaml.cs",
                $"{pathToProject}/.gitignore",
                $"{pathToProject}/.vs",
                $"{pathToProject}/folder/f1.cs",
                $"{pathToProject}/folder/f2.cs",
                $"{pathToProject}/res/f3.cs",
                $"{pathToProject}/res/images/img.png",
            };

            IList<PathFilter> filters = new List<PathFilter>();
            foreach (var extension in fileExtensions)
            {
                filters.Add(path => path.EndsWith(extension));
            }

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(pathsList);

            foreach (string path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            var projectModel = _sut.LoadSolution(pathToProject, filters);

            Assert.NotNull(projectModel);

            foreach (string path in pathsList)
            {
                if (fileExtensions.Any(extension => path.EndsWith(extension)))
                {
                    _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
                }
                else
                {
                    _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Never);
                }
            }
        }
        
        [Fact]
        public void LoadProjectTest_ShouldIgnoreFolderPathsFromFolder_WhenGivenValidPathToAProject()
        {
            string pathToProject = "validPathToProject";
            var pathsList = new List<string>
            {
                $"{pathToProject}/file1.cs",
                $"{pathToProject}/folder",
                $"{pathToProject}/folder/dir",
                $"{pathToProject}/folder/Class.cs",
                $"{pathToProject}/folder/Class2.cs",
                $"{pathToProject}/folder/dir/dir2",
                $"{pathToProject}/.gitignore",
                $"{pathToProject}/.vs",
                $"{pathToProject}/folder/f1.cs",
                $"{pathToProject}/folder/f2.cs",
                $"{pathToProject}/.config",
                $"{pathToProject}/res/f3.cs",
                $"{pathToProject}/res/images/img.png",
            };

            IList<PathFilter> filters = new List<PathFilter>();
            filters.Add(path => path.EndsWith(".cs"));
            filters.Add(path => path.EndsWith(".png"));
            filters.Add(path => path.EndsWith(".config"));

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(pathsList);

            foreach (string path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            var projectModel = _sut.LoadSolution(pathToProject, filters);

            Assert.NotNull(projectModel);

            foreach (string path in pathsList)
            {
                if (path.EndsWith(".cs") || path.EndsWith(".png") || path.EndsWith(".config"))
                {
                    _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
                }
                else
                {
                    _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Never);
                }
            }
        }
    }
}