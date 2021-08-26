using System.Text.Json;
using HoneydewModels.CSharp;
using HoneydewModels.Importers;
using Newtonsoft.Json;
using Xunit;

namespace HoneydewCoreIntegrationTests.IO.Models.Importers
{
    public class JsonSolutionModelImporterTests
    {
        private readonly IModelImporter<SolutionModel> _sut;

        public JsonSolutionModelImporterTests()
        {
            _sut = new JsonSolutionModelImporter();
        }

        [Theory(Skip = "Fix Later")]
        [InlineData(@"""Value"":4}")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(@"{Projects"":1")]
        [InlineData(@"{""Value"":1")]
        public void Import_ShouldThrowJsonException_WhenProvidedWithInvalidJsonFile(string content)
        {
            Assert.Throws<JsonSerializationException>(() => _sut.Import(content));
        }

        [Fact]
        public void Import_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
        {
            const string fileContent = @"{""a"":1}";

            var loadModelFromFile = _sut.Import(fileContent);

            Assert.Empty(loadModelFromFile.Projects);
        }

        [Fact(Skip = "Fix Later")]
        public void Import_ShouldReturnModel_WhenProvidedCorrectContent()
        {
            const string fileContent =
                @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""Name"":""SomeNamespace.FirstClass"",""BaseTypes"":[{""Type"":{""Name"":""object""}, ""Kind"":""class""}],""Fields"":[],""Metrics"":[{""ExtractorName"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric"",""ValueType"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}]}]}";

            var loadModelFromFile = _sut.Import(fileContent);

            Assert.NotNull(loadModelFromFile);
            Assert.Equal(1, loadModelFromFile.Projects.Count);
            Assert.Equal("ProjectName", loadModelFromFile.Projects[0].Name);

            Assert.Equal(1, loadModelFromFile.Projects[0].Namespaces.Count);
            var projectNamespace = loadModelFromFile.Projects[0].Namespaces[0];

            Assert.Equal("SomeNamespace", projectNamespace.Name);
            Assert.Equal(1, projectNamespace.ClassModels.Count);
            var classModel = (ClassModel)projectNamespace.ClassModels[0];

            Assert.Equal("SomePath", classModel.FilePath);
            Assert.Equal("SomeNamespace.FirstClass", classModel.Name);
            Assert.Equal("object", classModel.BaseTypes[0].Type.Name);
            Assert.Equal("class", classModel.BaseTypes[0].Kind);
            Assert.Empty(classModel.Fields);
            Assert.Empty(classModel.Methods);
            Assert.Equal(1, classModel.Metrics.Count);
            Assert.Equal("HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric",
                classModel.Metrics[0].ExtractorName);
            Assert.Equal("HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric",
                classModel.Metrics[0].ValueType);

            Assert.Equal(typeof(JsonElement), classModel.Metrics[0].Value.GetType());
            var value = (JsonElement)classModel.Metrics[0].Value;

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
