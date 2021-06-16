using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
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
            _sut = new MsBuildSolutionReader(new List<IFactExtractor>());
        }
        
        [Fact(Skip = "RegisterInstance was called, but MSBuild assemblies were already loaded.")]
        public void LoadSolution_ShouldThrowProjectNotFoundException_WhenGivenAnInvalidPath()
        {
            const string pathToProject = "invalidPathToProject";
            _fileReaderMock.Setup(reader => reader.ReadFilePaths(pathToProject)).Returns(new List<string>());

            var fileNotFoundException =
                Assert.Throws<ProjectNotFoundException>(() =>
                    _sut.LoadSolution(pathToProject, _solutionLoadingStrategy.Object));
            Assert.Equal("Project not found at specified Path", fileNotFoundException.Message);
        }

        [Fact(Skip = "RegisterInstance was called, but MSBuild assemblies were already loaded.")]
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

            foreach (var path in pathsList)
            {
                _fileReaderMock.Setup(reader => reader.ReadFile(path)).Returns("");
            }

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(string.Empty, new List<IFactExtractor>()))
                .Returns(() => new List<ProjectClassModel>
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

        [Fact(Skip = "RegisterInstance was called, but MSBuild assemblies were already loaded.")]
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
                .Returns(() => new List<ProjectClassModel>
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
        
        [Theory(Skip = "RegisterInstance was called, but MSBuild assemblies were already loaded.")]
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
                .Returns(() => new List<ProjectClassModel>
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

        [Fact(Skip = "RegisterInstance was called, but MSBuild assemblies were already loaded.")]
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
                .Returns(() => new List<ProjectClassModel>
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

        [Fact(Skip = "RegisterInstance was called, but MSBuild assemblies were already loaded.")]
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
                .Returns((() => new List<ProjectClassModel>
                {
                    new()
                    {
                        FullName = "IO.Readers.Reader1", FilePath = reader1ClassPath
                    }
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(reader2Class, new List<IFactExtractor>()))
                .Returns((() => new List<ProjectClassModel>
                {
                    new()
                    {
                        FullName = "IO.Readers.Reader2", FilePath = reader2ClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(writerClass, new List<IFactExtractor>()))
                .Returns((() => new List<ProjectClassModel>
                {
                    new()
                    {
                        FullName = "IO.Writers.Writer", FilePath = writerClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(model1Class, new List<IFactExtractor>()))
                .Returns((() => new List<ProjectClassModel>
                {
                    new()
                    {
                        FullName = "Models.Model1", FilePath = model1ClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(model2Class, new List<IFactExtractor>()))
                .Returns((() => new List<ProjectClassModel>
                {
                    new()
                    {
                        FullName = "Models.Model2", FilePath = model2ClassPath
                    },
                }));

            _solutionLoadingStrategy.Setup(strategy => strategy.Load(serviceClass, new List<IFactExtractor>()))
                .Returns((() => new List<ProjectClassModel>
                {
                    new()
                    {
                        FullName = "Services.Service", FilePath = serviceClassPath
                    },
                }));

            var projectModel = _sut.LoadSolution(projectPath, _solutionLoadingStrategy.Object);

            Assert.NotNull(projectModel);

            foreach (var path in pathsList)
            {
                _fileReaderMock.Verify(reader => reader.ReadFile(path), Times.Once);
            }

            Assert.Equal(4, projectModel.Namespaces.Count);

            var ioReadersProjectNamespace = projectModel.Namespaces["IO.Readers"];
            Assert.Equal("IO.Readers", ioReadersProjectNamespace.Name);
            Assert.Equal(2, ioReadersProjectNamespace.ClassModels.Count);
            Assert.Equal("IO.Readers.Reader1", ioReadersProjectNamespace.ClassModels[0].FullName);
            Assert.Equal(reader1ClassPath, ioReadersProjectNamespace.ClassModels[0].FilePath);
            Assert.Equal("IO.Readers.Reader2", ioReadersProjectNamespace.ClassModels[1].FullName);
            Assert.Equal(reader2ClassPath, ioReadersProjectNamespace.ClassModels[1].FilePath);

            var ioWritersProjectModelNamespace = projectModel.Namespaces["IO.Writers"];
            Assert.Equal("IO.Writers", ioWritersProjectModelNamespace.Name);
            Assert.Equal(1, ioWritersProjectModelNamespace.ClassModels.Count);
            Assert.Equal("IO.Writers.Writer", ioWritersProjectModelNamespace.ClassModels[0].FullName);
            Assert.Equal(writerClassPath, ioWritersProjectModelNamespace.ClassModels[0].FilePath);

            var modelsProjectModelNamespace = projectModel.Namespaces["Models"];
            Assert.Equal("Models", modelsProjectModelNamespace.Name);
            Assert.Equal(2, modelsProjectModelNamespace.ClassModels.Count);
            Assert.Equal("Models.Model1", modelsProjectModelNamespace.ClassModels[0].FullName);
            Assert.Equal(model1ClassPath, modelsProjectModelNamespace.ClassModels[0].FilePath);
            Assert.Equal("Models.Model2", modelsProjectModelNamespace.ClassModels[1].FullName);
            Assert.Equal(model2ClassPath, modelsProjectModelNamespace.ClassModels[1].FilePath);

            var servicesProjectModelNamespace = projectModel.Namespaces["Services"];
            Assert.Equal("Services", servicesProjectModelNamespace.Name);
            Assert.Equal(1, servicesProjectModelNamespace.ClassModels.Count);
            Assert.Equal("Services.Service", servicesProjectModelNamespace.ClassModels[0].FullName);
            Assert.Equal(serviceClassPath, servicesProjectModelNamespace.ClassModels[0].FilePath);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnNull_WhenEmptyFileIsProvided()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns("");

            var loadModelFromFile = _sut.LoadModelFromFile(_fileReaderMock.Object, pathToModel);

            Assert.Null(loadModelFromFile);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(@"{""a"":1}");

            var loadModelFromFile = _sut.LoadModelFromFile(_fileReaderMock.Object, pathToModel);

            Assert.Empty(loadModelFromFile.Namespaces);
        }


        [Fact]
        public void LoadModelFromFile_ShouldReturnModel_WhenProvidedCorrectContent()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel))
                .Returns(
                    @"{""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}}}");

            var loadModelFromFile = _sut.LoadModelFromFile(_fileReaderMock.Object, pathToModel);

            Assert.NotNull(loadModelFromFile);
            Assert.Equal(1, loadModelFromFile.Namespaces.Count);
            var projectNamespace = loadModelFromFile.Namespaces["SomeNamespace"];

            Assert.Equal("SomeNamespace", projectNamespace.Name);
            Assert.Equal(1, projectNamespace.ClassModels.Count);
            var projectNamespaceClassModel = projectNamespace.ClassModels[0];

            Assert.Equal("SomePath", projectNamespaceClassModel.FilePath);
            Assert.Equal("SomeNamespace.FirstClass", projectNamespaceClassModel.FullName);
            Assert.Equal(1, projectNamespaceClassModel.Metrics.Count);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric",
                projectNamespaceClassModel.Metrics[0].ExtractorName);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric",
                projectNamespaceClassModel.Metrics[0].ValueType);
            Assert.Equal(typeof(InheritanceMetric), projectNamespaceClassModel.Metrics[0].Value.GetType());
            var value = (InheritanceMetric) projectNamespaceClassModel.Metrics[0].Value;
            Assert.Equal(1, value.Interfaces.Count);
            Assert.Equal("Interface1", value.Interfaces[0]);
            Assert.Equal("SomeParent", value.BaseClassName);
        }
    }
}