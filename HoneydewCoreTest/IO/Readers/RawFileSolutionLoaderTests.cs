using System.Threading.Tasks;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.SolutionRead;
using Moq;
using Xunit;

namespace HoneydewCoreTest.IO.Readers
{
    public class RawFileSolutionLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<IFileReader> _fileReaderMock = new();

        public RawFileSolutionLoaderTests()
        {
            _sut = new RawFileSolutionLoader(_fileReaderMock.Object);
        }


        [Fact]
        public void LoadModelFromFile_ShouldReturnNull_WhenEmptyFileIsProvided()
        {
            const string pathToModel = "pathToModel";
            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns("");

            var loadModelFromFile = _sut.LoadSolution(pathToModel);

            Assert.Null(loadModelFromFile);
        }

        [Fact]
        public void LoadModelFromFile_ShouldReturnNull_WhenProvidedWithInvalidJsonFile()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(@"{Projects"":1");

            var loadModelFromFile = _sut.LoadSolution(pathToModel);

            Assert.Null(loadModelFromFile);
        }

        [Fact]
        public async Task LoadModelFromFile_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(@"{""a"":1}");

            var loadModelFromFile = await _sut.LoadSolution(pathToModel);

            Assert.Empty(loadModelFromFile.Projects);
        }

        [Fact]
        public async Task LoadModelFromFile_ShouldReturnModel_WhenProvidedCorrectContent()
        {
            const string pathToModel = "pathToModel";

            _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel))
                .Returns(
                    @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""BaseClassFullName"":""object"",""Fields"":[],""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}}}]}");

            var loadModelFromFile = await _sut.LoadSolution(pathToModel);

            Assert.NotNull(loadModelFromFile);
            Assert.Equal(1, loadModelFromFile.Projects.Count);
            Assert.Equal("ProjectName", loadModelFromFile.Projects[0].Name);

            Assert.Equal(1, loadModelFromFile.Projects[0].Namespaces.Count);
            var projectNamespace = loadModelFromFile.Projects[0].Namespaces["SomeNamespace"];

            Assert.Equal("SomeNamespace", projectNamespace.Name);
            Assert.Equal(1, projectNamespace.ClassModels.Count);
            var classModel = projectNamespace.ClassModels[0];

            Assert.Equal("SomePath", classModel.FilePath);
            Assert.Equal("SomeNamespace.FirstClass", classModel.FullName);
            Assert.Equal("object", classModel.BaseClassFullName);
            Assert.Empty(classModel.Fields);
            Assert.Empty(classModel.Methods);
            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric",
                classModel.Metrics[0].ExtractorName);
            Assert.Equal("HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric",
                classModel.Metrics[0].ValueType);
            Assert.Equal(typeof(InheritanceMetric), classModel.Metrics[0].Value.GetType());
            var value = (InheritanceMetric) classModel.Metrics[0].Value;
            Assert.Equal(1, value.Interfaces.Count);
            Assert.Equal("Interface1", value.Interfaces[0]);
            Assert.Equal("SomeParent", value.BaseClassName);
        }
    }
}