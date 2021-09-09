// using System.Collections.Generic;
// using System.Text.Json;
// using System.Threading.Tasks;
// using HoneydewCore.IO.Readers;
// using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
// using HoneydewModels.CSharp;
// using HoneydewModels.Importers;
// using HoneydewModels.Types;
// using Moq;
// using Newtonsoft.Json;
// using Xunit;
// using JsonSerializer = System.Text.Json.JsonSerializer;
//
// namespace HoneydewExtractorsTests.CSharp.RepositoryLoading
// {
//     public class RawFileSolutionLoaderTests
//     {
//         private readonly ISolutionLoader _sut;
//
//         private readonly Mock<IFileReader> _fileReaderMock = new();
//         private readonly Mock<JsonModelImporter<SolutionModel>> _solutionModelImporterMock = new();
//
//         public RawFileSolutionLoaderTests()
//         {
//             _sut = new RawFileSolutionLoader(_fileReaderMock.Object, _solutionModelImporterMock.Object);
//         }
//
//         [Fact]
//         public void LoadModelFromFile_ShouldReturnNull_WhenEmptyFileIsProvided()
//         {
//             const string pathToModel = "pathToModel";
//             _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns("");
//
//             _solutionModelImporterMock.Setup(importer => importer.Import("")).Returns((SolutionModel)null);
//
//             var loadModelFromFile = _sut.LoadSolution(pathToModel);
//
//             Assert.Null(loadModelFromFile);
//         }
//
//         [Fact]
//         public void LoadModelFromFile_ShouldReturnNull_WhenProvidedWithInvalidJsonFile()
//         {
//             const string pathToModel = "pathToModel";
//
//             const string fileContent = @"{Projects"":1";
//             _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(fileContent);
//
//             _solutionModelImporterMock.Setup(importer => importer.Import(fileContent)).Returns((SolutionModel)null);
//
//             var loadModelFromFile = _sut.LoadSolution(pathToModel);
//
//             Assert.Null(loadModelFromFile);
//         }
//
//         [Fact]
//         public async Task LoadModelFromFile_ShouldReturnEmptyModel_WhenProvidedContentToOtherJSON()
//         {
//             const string pathToModel = "pathToModel";
//
//             const string fileContent = @"{""a"":1}";
//             _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(fileContent);
//
//             _solutionModelImporterMock.Setup(importer => importer.Import(fileContent)).Returns(new SolutionModel());
//
//             var loadModelFromFile = await _sut.LoadSolution(pathToModel);
//
//             Assert.Empty(loadModelFromFile.Projects);
//         }
//
//         [Fact]
//         public async Task LoadModelFromFile_ShouldReturnModel_WhenProvidedCorrectContent()
//         {
//             const string pathToModel = "pathToModel";
//
//             const string fileContent =
//                 @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""BaseClassFullName"":""object"",""Fields"":[],""Metrics"":[{""ExtractorName"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric"",""ValueType"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}]}]}";
//
//
//             var baseTypesList = new List<IBaseType>
//             {
//                 new BaseTypeModel
//                 {
//                     Type = new EntityTypeModel
//                     {
//                         Name = "SomeParent"
//                     },
//                     Kind = "class"
//                 },
//                 new BaseTypeModel
//                 {
//                     Type = new EntityTypeModel
//                     {
//                         Name = "Interface1"
//                     },
//                     Kind = "interface"
//                 }
//             };
//
//
//             var jsonElement =
//                 JsonSerializer.Deserialize<object>(JsonConvert.SerializeObject(baseTypesList, Formatting.None));
//
//
//             var solutionModel = new SolutionModel
//             {
//                 Projects = new List<ProjectModel>
//                 {
//                     new()
//                     {
//                         Name = "ProjectName",
//                         Namespaces =
//                         {
//                             new NamespaceModel
//                             {
//                                 Name = "SomeNamespace",
//                                 ClassModels =
//                                 {
//                                     new ClassModel
//                                     {
//                                         FilePath = "SomePath",
//                                         Name = "SomeNamespace.FirstClass",
//                                         BaseTypes = new List<IBaseType>
//                                         {
//                                             new BaseTypeModel
//                                             {
//                                                 Type = new EntityTypeModel
//                                                 {
//                                                     Name = "object"
//                                                 },
//                                                 Kind = "class"
//                                             }
//                                         },
//                                         Metrics =
//                                         {
//                                             new MetricModel
//                                             {
//                                                 ExtractorName =
//                                                     "HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric",
//                                                 ValueType = typeof(List<IBaseType>).FullName,
//                                                 Value = jsonElement
//                                             }
//                                         }
//                                     }
//                                 }
//                             }
//                         }
//                     }
//                 }
//             };
//
//             _fileReaderMock.Setup(reader => reader.ReadFile(pathToModel)).Returns(fileContent);
//
//             _solutionModelImporterMock.Setup(importer => importer.Import(fileContent)).Returns(solutionModel);
//
//             var loadModelFromFile = await _sut.LoadSolution(pathToModel);
//
//             Assert.NotNull(loadModelFromFile);
//             Assert.Equal(1, loadModelFromFile.Projects.Count);
//             Assert.Equal("ProjectName", loadModelFromFile.Projects[0].Name);
//
//             Assert.Equal(1, loadModelFromFile.Projects[0].Namespaces.Count);
//             var projectNamespace = loadModelFromFile.Projects[0].Namespaces[0];
//
//             Assert.Equal("SomeNamespace", projectNamespace.Name);
//             Assert.Equal(1, projectNamespace.ClassModels.Count);
//             var classModel = (ClassModel)projectNamespace.ClassModels[0];
//
//             Assert.Equal("SomePath", classModel.FilePath);
//             Assert.Equal("SomeNamespace.FirstClass", classModel.Name);
//             Assert.Equal("object", classModel.BaseTypes[0].Type.Name);
//             Assert.Equal("class", classModel.BaseTypes[0].Kind);
//             Assert.Empty(classModel.Fields);
//             Assert.Empty(classModel.Methods);
//             Assert.Equal(1, classModel.Metrics.Count);
//             Assert.Equal("HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric",
//                 classModel.Metrics[0].ExtractorName);
//             Assert.Equal(typeof(List<IBaseType>).FullName, classModel.Metrics[0].ValueType);
//
//             Assert.Equal(typeof(JsonElement), classModel.Metrics[0].Value.GetType());
//             var baseTypes = (JsonElement)classModel.Metrics[0].Value;
//
//             Assert.Equal(2, baseTypes.GetArrayLength());
//             var arrayEnumerator = baseTypes.EnumerateArray();
//
//             arrayEnumerator.MoveNext();
//
//             var baseClass = arrayEnumerator.Current;
//             var baseClassElement = baseClass.GetProperty("Type");
//             Assert.Equal("SomeParent", baseClassElement.GetProperty("Name").GetString());
//             Assert.Equal("class", baseClass.GetProperty("Kind").GetString());
//
//             arrayEnumerator.MoveNext();
//
//             var baseInterface = arrayEnumerator.Current;
//             var baseInterfaceElement = baseInterface.GetProperty("Type");
//             Assert.Equal("Interface1", baseInterfaceElement.GetProperty("Name").GetString());
//             Assert.Equal("interface", baseInterface.GetProperty("Kind").GetString());
//         }
//     }
// }
