using System.Text.Json;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Importers;
using Xunit;

namespace HoneydewCoreIntegrationTests.IO.Models.Importers
{
    public class JsonSolutionModelImporterTests
    {
        private readonly IModelImporter<SolutionModel> _sut;

        public JsonSolutionModelImporterTests()
        {
            _sut = new JsonSolutionModelImporter(new ConverterList());
        }

        [Fact]
        public void Import_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
        {
            const string fileContent = @"{""a"":1}";

            var loadModelFromFile = _sut.Import(fileContent);

            Assert.Empty(loadModelFromFile.Projects);
        }

        [Fact]
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

        [Fact]
        public void Import_ShouldReturnModel_WhenProvidedDelegateAndClassModel()
        {
            const string fileContent =
                @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""Name"":""Class1"",""FilePath"":""path/Class1.cs"",""Loc"":{""SourceLines"":28,""CommentedLines"":0,""EmptyLines"":5},""AccessModifier"":""public"",""Modifier"":"""",""ContainingTypeName"":""Namespace1"",""BaseTypes"":[{""Type"":{""Name"":""object"",""ContainedTypes"":[]},""Kind"":""class""}],""Imports"":[],""Fields"":[],""Properties"":[],""Constructors"":[],""Methods"":[]},{""ClassType"":""delegate"",""FilePath"":""path/Delegate1.cs"",""Name"":""Delegate1"",""BaseTypes"":[{""Type"":{""Name"":""System.Delegate"",""ContainedTypes"":[]},""Kind"":""class""}],""Imports"":[],""ContainingTypeName"":""Namespace1"",""AccessModifier"":""public"",""Modifier"":"""",""Attributes"":[],""ParameterTypes"":[],""ReturnValue"":{""Type"":{""Name"":""void"",""ContainedTypes"":[]},""Modifier"":"""",""Attributes"":[]},""Metrics"":[]}]}]}]}";

            var loadModelFromFile = _sut.Import(fileContent);

            Assert.NotNull(loadModelFromFile);
            Assert.Equal(1, loadModelFromFile.Projects.Count);
            Assert.Equal("ProjectName", loadModelFromFile.Projects[0].Name);

            Assert.Equal(1, loadModelFromFile.Projects[0].Namespaces.Count);
            var projectNamespace = loadModelFromFile.Projects[0].Namespaces[0];

            Assert.Equal("SomeNamespace", projectNamespace.Name);
            Assert.Equal(2, projectNamespace.ClassModels.Count);
            Assert.Equal(typeof(ClassModel), projectNamespace.ClassModels[0].GetType());
            Assert.Equal(typeof(DelegateModel), projectNamespace.ClassModels[1].GetType());
        }
    }
}
