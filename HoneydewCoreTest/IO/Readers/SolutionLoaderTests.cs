using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Models;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.IO.Readers.Strategies;
using Moq;
using Xunit;

namespace HoneydewCoreTest.IO.Readers
{
    public class SolutionLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<IFileReader> _fileReaderMock = new();
        private readonly Mock<ISolutionLoadingStrategy> _solutionLoadingStrategy = new();

        public SolutionLoaderTests()
        {
            _sut = new SolutionLoader(_fileReaderMock.Object, new List<IFactExtractor>());
        }

        [Fact]
        public void LoadSolution_ShouldThrowProjectNotFoundException_WhenGivenAnInvalidPath()
        {
            const string pathToProject = "invalidPathToProject";
            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(new List<string>());

            var fileNotFoundException =
                Assert.Throws<ProjectNotFoundException>(() =>
                    _sut.LoadSolution(pathToProject, _solutionLoadingStrategy.Object));
            Assert.Equal("Project not found at specified Path", fileNotFoundException.Message);
        }

        [Fact]
        public void LoadSolution_ShouldReadAllFilesFromFolder_WhenGivenAValidPathToAProject()
        {
            const string pathToProject = "validPathToProject";
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

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(string.Empty, new List<IFactExtractor>()))
                .Returns(() => new List<ClassModel>
                {
                    new()
                });

            var projectModel = _sut.LoadSolution(pathToProject, _solutionLoadingStrategy.Object);

            Assert.NotNull(projectModel);

            foreach (var path in pathsList)
            {
                _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
            }
        }

        [Fact]
        public void LoadSolution_ShouldReadAllCSFilesFromFolder_WhenGivenValidPathToAProject_AndOneFilter()
        {
            const string pathToProject = "validPathToProject";

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

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject))
                .Returns(pathsList.Where(path => path.EndsWith(".cs")).ToList());

            foreach (var path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(string.Empty, new List<IFactExtractor>()))
                .Returns(() => new List<ClassModel>
                {
                    new()
                });

            var projectModel = _sut.LoadSolution(pathToProject, _solutionLoadingStrategy.Object);

            Assert.NotNull(projectModel);

            foreach (var path in pathsList)
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
        public void LoadSolution_ShouldReadAllFilteredFilesWithExtensionFromFolder_WhenGivenValidPathToAProject(
            string[] fileExtensions)
        {
            const string pathToProject = "validPathToProject";

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

            var filters = fileExtensions.Select(extension => (PathFilter) (path => path.EndsWith(extension))).ToList();

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject))
                .Returns(pathsList.Where(path => { return filters.Any(filter => filter(path)); }).ToList());

            foreach (var path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(string.Empty, new List<IFactExtractor>()))
                .Returns(() => new List<ClassModel>
                {
                    new()
                });

            var projectModel = _sut.LoadSolution(pathToProject, _solutionLoadingStrategy.Object);

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
        public void LoadSolution_ShouldIgnoreFolderPathsFromFolder_WhenGivenValidPathToAProject()
        {
            const string pathToProject = "validPathToProject";
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

            var filters = new List<PathFilter>
            {
                path => path.EndsWith(".cs"),
                path => path.EndsWith(".png"),
                path => path.EndsWith(".config")
            };

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject))
                .Returns(pathsList.Where(path => { return filters.Any(filter => filter(path)); }).ToList());

            foreach (var path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(string.Empty, new List<IFactExtractor>()))
                .Returns(() => new List<ClassModel>
                {
                    new()
                });

            var projectModel = _sut.LoadSolution(pathToProject, _solutionLoadingStrategy.Object);

            Assert.NotNull(projectModel);

            foreach (var path in pathsList)
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

        [Fact]
        public void LoadSolution_ShouldHaveClassModelsWithCorrectPath_WhenGivenAValidPathToAProject()
        {
            const string projectPath = "validPathToProject";

            const string reader1ClassPath = "validPathToProject/IO/Readers/Reader1.cs";
            const string reader2ClassPath = "validPathToProject/IO/Readers/Reader2.cs";
            const string writerClassPath = "validPathToProject/IO/Writers/Writer.cs";
            const string model1ClassPath = "validPathToProject/Models/Model1.cs";
            const string model2ClassPath = "validPathToProject/Models/Model2.cs";
            const string serviceClassPath = "validPathToProject/Services/Service.cs";

            const string reader1Class = "namespace IO.Readers {class Reader1{}}";
            const string reader2Class = "namespace IO.Readers {class Reader2{}}";
            const string writerClass = "namespace IO.Writers {class Writer{}}";
            const string model1Class = "namespace Models {class Model1{}}";
            const string model2Class = "namespace Models {class Model2{}}";
            const string serviceClass = "namespace Services {class Service{}}";

            var pathsList = new List<string>
            {
                reader1ClassPath,
                reader2ClassPath,
                writerClassPath,
                model1ClassPath,
                model2ClassPath,
                serviceClassPath,
            };

            _fileReaderMock.Setup(reader => reader.ReadFilePaths(projectPath))
                .Returns(pathsList);

            _fileReaderMock.Setup(reader => reader.ReadFile(reader1ClassPath)).Returns(reader1Class);
            _fileReaderMock.Setup(reader => reader.ReadFile(reader2ClassPath)).Returns(reader2Class);
            _fileReaderMock.Setup(reader => reader.ReadFile(writerClassPath)).Returns(writerClass);
            _fileReaderMock.Setup(reader => reader.ReadFile(model1ClassPath)).Returns(model1Class);
            _fileReaderMock.Setup(reader => reader.ReadFile(model2ClassPath)).Returns(model2Class);
            _fileReaderMock.Setup(reader => reader.ReadFile(serviceClassPath)).Returns(serviceClass);

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(reader1Class, new List<IFactExtractor>()))
                .Returns((() => new List<ClassModel>
                {
                    new()
                    {
                        Name = "Reader1", Namespace = "IO.Readers", FilePath = reader1ClassPath
                    }
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(reader2Class, new List<IFactExtractor>()))
                .Returns((() => new List<ClassModel>
                {
                    new()
                    {
                        Name = "Reader2", Namespace = "IO.Readers", FilePath = reader2ClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(writerClass, new List<IFactExtractor>()))
                .Returns((() => new List<ClassModel>
                {
                    new()
                    {
                        Name = "Writer", Namespace = "IO.Writers", FilePath = writerClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(model1Class, new List<IFactExtractor>()))
                .Returns((() => new List<ClassModel>
                {
                    new()
                    {
                        Name = "Model1", Namespace = "Models", FilePath = model1ClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(model2Class, new List<IFactExtractor>()))
                .Returns((() => new List<ClassModel>
                {
                    new()
                    {
                        Name = "Model2", Namespace = "Models", FilePath = model2ClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(serviceClass, new List<IFactExtractor>()))
                .Returns((() => new List<ClassModel>
                {
                    new()
                    {
                        Name = "Service", Namespace = "Services", FilePath = serviceClassPath
                    },
                }));

            var projectModel = _sut.LoadSolution(projectPath, _solutionLoadingStrategy.Object);

            Assert.NotNull(projectModel);

            foreach (var path in pathsList)
            {
                _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
            }

            Assert.Equal(4, projectModel.Namespaces.Count);

            Assert.Equal("IO.Readers", projectModel.Namespaces[0].Name);
            Assert.Equal(2, projectModel.Namespaces[0].ClassModels.Count);
            Assert.Equal("IO.Readers.Reader1", projectModel.Namespaces[0].ClassModels[0].FullName);
            Assert.Equal(reader1ClassPath, projectModel.Namespaces[0].ClassModels[0].Path);
            Assert.Equal("IO.Readers.Reader2", projectModel.Namespaces[0].ClassModels[1].FullName);
            Assert.Equal(reader2ClassPath, projectModel.Namespaces[0].ClassModels[1].Path);

            Assert.Equal("IO.Writers", projectModel.Namespaces[1].Name);
            Assert.Equal(1, projectModel.Namespaces[1].ClassModels.Count);
            Assert.Equal("IO.Writers.Writer", projectModel.Namespaces[1].ClassModels[0].FullName);
            Assert.Equal(writerClassPath, projectModel.Namespaces[1].ClassModels[0].Path);

            Assert.Equal("Models", projectModel.Namespaces[2].Name);
            Assert.Equal(2, projectModel.Namespaces[2].ClassModels.Count);
            Assert.Equal("Models.Model1", projectModel.Namespaces[2].ClassModels[0].FullName);
            Assert.Equal(model1ClassPath, projectModel.Namespaces[2].ClassModels[0].Path);
            Assert.Equal("Models.Model2", projectModel.Namespaces[2].ClassModels[1].FullName);
            Assert.Equal(model2ClassPath, projectModel.Namespaces[2].ClassModels[1].Path);

            Assert.Equal("Services", projectModel.Namespaces[3].Name);
            Assert.Equal(1, projectModel.Namespaces[3].ClassModels.Count);
            Assert.Equal("Services.Service", projectModel.Namespaces[3].ClassModels[0].FullName);
            Assert.Equal(serviceClassPath, projectModel.Namespaces[3].ClassModels[0].Path);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnNull_WhenEmptyFileIsProvided()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns("");

            var loadModelFromFile = _sut.LoadModelFromFile(pathToModel);

            Assert.Null(loadModelFromFile);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(@"{""a"":1}");

            var loadModelFromFile = _sut.LoadModelFromFile(pathToModel);

            Assert.Empty(loadModelFromFile.Namespaces);
        }

        
        [Fact]
        public void LoadModelFromFile_ShouldReturnModel_WhenProvidedCorrectContent()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel))
                .Returns(
                    @"{""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""Path"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}]}");

            var loadModelFromFile = _sut.LoadModelFromFile(pathToModel);

            Assert.NotNull(loadModelFromFile);
            Assert.Equal(1, loadModelFromFile.Namespaces.Count);
            Assert.Equal("SomeNamespace", loadModelFromFile.Namespaces[0].Name);
            Assert.Equal(1, loadModelFromFile.Namespaces[0].ClassModels.Count);
            Assert.Equal("SomePath", loadModelFromFile.Namespaces[0].ClassModels[0].Path);
            Assert.Equal("SomeNamespace.FirstClass", loadModelFromFile.Namespaces[0].ClassModels[0].FullName);
            Assert.Equal(1, loadModelFromFile.Namespaces[0].ClassModels[0].Metrics.Count);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric",
                loadModelFromFile.Namespaces[0].ClassModels[0].Metrics[0].ExtractorName);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric",
                loadModelFromFile.Namespaces[0].ClassModels[0].Metrics[0].ValueType);
            Assert.Equal(typeof(InheritanceMetric), loadModelFromFile.Namespaces[0].ClassModels[0].Metrics[0].Value.GetType());
            var value = (InheritanceMetric) loadModelFromFile.Namespaces[0].ClassModels[0].Metrics[0].Value;
            Assert.Equal(1, value.Interfaces.Count);
            Assert.Equal("Interface1", value.Interfaces[0]);
            Assert.Equal("SomeParent", value.BaseClassName);
        }
    }
}