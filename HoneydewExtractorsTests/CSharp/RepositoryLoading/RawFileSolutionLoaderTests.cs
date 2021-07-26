using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using HoneydewCore.IO.Readers;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewModels.CSharp;
using HoneydewModels.Importers;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.RepositoryLoading
{
    public class RawFileSolutionLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<IFileReader> _fileReaderMock = new();
        private readonly Mock<IModelImporter<SolutionModel>> _solutionModelImporterMock = new();

        public RawFileSolutionLoaderTests()
        {
            _sut = new RawFileSolutionLoader(_fileReaderMock.Object, _solutionModelImporterMock.Object);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnNull_WhenEmptyFileIsProvided()
        {
            const string pathToModel = "pathToModel";
            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns("");

            _solutionModelImporterMock.Setup(importer => importer.Import("")).Returns((SolutionModel) null);

            var loadModelFromFile = _sut.LoadSolution(pathToModel);

            Assert.Null(loadModelFromFile);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnNull_WhenProvidedWithInvalidJsonFile()
        {
            const string pathToModel = "pathToModel";

            const string fileContent = @"{Projects"":1";
            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(fileContent);

            _solutionModelImporterMock.Setup(importer => importer.Import(fileContent)).Returns((SolutionModel) null);

            var loadModelFromFile = _sut.LoadSolution(pathToModel);

            Assert.Null(loadModelFromFile);
        }

        [Fact]
        public async Task LoadModelFromFile_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
        {
            const string pathToModel = "pathToModel";

            const string fileContent = @"{""a"":1}";
            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(fileContent);

            _solutionModelImporterMock.Setup(importer => importer.Import(fileContent)).Returns(new SolutionModel());

            var loadModelFromFile = await _sut.LoadSolution(pathToModel);

            Assert.Empty(loadModelFromFile.Projects);
        }

        [Fact]
        public async Task LoadModelFromFile_ShouldReturnModel_WhenProvidedCorrectContent()
        {
            const string pathToModel = "pathToModel";

            const string fileContent =
                @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""BaseClassFullName"":""object"",""Fields"":[],""Metrics"":[{""ExtractorName"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric"",""ValueType"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}]}]}";


            var cSharpInheritanceMetric = new CSharpInheritanceMetric
            {
                BaseClassName = "SomeParent",
                Interfaces = new List<string>
                {
                    "Interface1"
                }
            };


            var jsonElement = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(cSharpInheritanceMetric));


            var solutionModel = new SolutionModel
            {
                Projects = new List<ProjectModel>
                {
                    new()
                    {
                        Name = "ProjectName",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                Name = "SomeNamespace",
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "SomePath",
                                        FullName = "SomeNamespace.FirstClass",
                                        BaseClassFullName = "object",
                                        Metrics =
                                        {
                                            new ClassMetric
                                            {
                                                ExtractorName =
                                                    "HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric",
                                                ValueType =
                                                    "HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric",
                                                Value = jsonElement
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(fileContent);

            _solutionModelImporterMock.Setup(importer => importer.Import(fileContent)).Returns(solutionModel);

            var loadModelFromFile = await _sut.LoadSolution(pathToModel);

            Assert.NotNull(loadModelFromFile);
            Assert.Equal(1, loadModelFromFile.Projects.Count);
            Assert.Equal("ProjectName", loadModelFromFile.Projects[0].Name);

            Assert.Equal(1, loadModelFromFile.Projects[0].Namespaces.Count);
            var projectNamespace = loadModelFromFile.Projects[0].Namespaces[0];

            Assert.Equal("SomeNamespace", projectNamespace.Name);
            Assert.Equal(1, projectNamespace.ClassModels.Count);
            var classModel = projectNamespace.ClassModels[0];

            Assert.Equal("SomePath", classModel.FilePath);
            Assert.Equal("SomeNamespace.FirstClass", classModel.FullName);
            Assert.Equal("object", classModel.BaseClassFullName);
            Assert.Empty(classModel.Fields);
            Assert.Empty(classModel.Methods);
            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric",
                classModel.Metrics[0].ExtractorName);
            Assert.Equal("HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric",
                classModel.Metrics[0].ValueType);

            Assert.Equal(typeof(JsonElement), classModel.Metrics[0].Value.GetType());
            var value = (JsonElement) classModel.Metrics[0].Value;

            var baseClassName = value.GetProperty("BaseClassName");
            Assert.Equal("SomeParent", baseClassName.GetString());

            var interfacesJsonElement = value.GetProperty("Interfaces");
            Assert.Equal(1, interfacesJsonElement.GetArrayLength());
            foreach (var element in interfacesJsonElement.EnumerateArray())
            {
                Assert.Equal("Interface1", element.GetString());
            }
        }
    }
}
