using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace HoneydewCoreTest.IO.Readers
{
    public class SolutionFileLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<IFileReader> _fileReaderMock = new();
        private readonly Mock<ISolutionProvider> _solutionProviderMock = new();
        private readonly Mock<ISolutionLoadingStrategy> _solutionLoadingStrategyMock = new();

        public SolutionFileLoaderTests()
        {
            _sut = new SolutionFileLoader(new List<IFactExtractor>(), _solutionProviderMock.Object,
                _solutionLoadingStrategyMock.Object);
        }

        [Fact]
        public void LoadSolution_ShouldThrowProjectNotFoundException_WhenGivenAnInvalidPath()
        {
            const string pathToSolution = "invalidPathToProject";

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution))
                .Throws<ProjectNotFoundException>();

            Assert.ThrowsAsync<ProjectNotFoundException>(() => _sut.LoadSolution(pathToSolution));
        }

        [Fact]
        public void LoadSolution_ShouldThrowProjectWithErrors_WhenGivenAProjectWithErrors()
        {
            const string pathToSolution = "validPathToProject";

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution))
                .Throws<ProjectWithErrorsException>();

            Assert.ThrowsAsync<ProjectWithErrorsException>(() => _sut.LoadSolution(pathToSolution));
        }

        [Fact]
        public void LoadSolution_ShouldReturnCorrectSolutionModel_WhenGivenAPathToSolution()
        {
            const string pathToSolution = "validPathToProject";
            var solutionModelMock = new Mock<SolutionModel>();

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution))
                .Returns(It.IsAny<Task<Solution>>());
            _solutionLoadingStrategyMock
                .Setup(strategy => strategy.Load(It.IsAny<Solution>(), new List<IFactExtractor>()))
                .ReturnsAsync(solutionModelMock.Object);

            var loadSolution = _sut.LoadSolution(pathToSolution);

            Assert.NotNull(loadSolution);
        }

        [Fact]
        public async Task LoadSolution_ShouldReturnCorrectSolutionModelWithMethodReferences_WhenGivenAPathToSolution()
        {
            const string pathToSolution = "validPathToProject";
            var solutionModelMock = new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        Name = "Project1",
                        Namespaces =
                        {
                            {
                                "Project1.Services", new NamespaceModel
                                {
                                    Name = "Project1.Services",
                                    ClassModels = new List<ClassModel>
                                    {
                                        new()
                                        {
                                            FullName = "CreateService",
                                            FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                            BaseClassFullName = "object",
                                            BaseInterfaces = {"IService"},
                                            Methods = new List<MethodModel>
                                            {
                                                new()
                                                {
                                                    Name = "A",
                                                    Modifier = "",
                                                    AccessModifier = "public",
                                                    ParameterTypes =
                                                    {
                                                        new ParameterModel
                                                        {
                                                            Type = "string"
                                                        }
                                                    },
                                                    ReturnType = "int",
                                                    ContainingClassName = "Project1.Services.CreateService",
                                                    CalledMethods =
                                                    {
                                                        new MethodCallModel
                                                        {
                                                            ContainingClassName = "Project1.Services.CreateService",
                                                            MethodName = "Convert"
                                                        }
                                                    }
                                                },
                                                new()
                                                {
                                                    Name = "Convert",
                                                    Modifier = "static",
                                                    AccessModifier = "public",
                                                    ParameterTypes =
                                                    {
                                                        new ParameterModel
                                                        {
                                                            Type = "string"
                                                        }
                                                    },
                                                    ReturnType = "int",
                                                    ContainingClassName = "Project1.Services.CreateService",
                                                    CalledMethods =
                                                    {
                                                        new MethodCallModel
                                                        {
                                                            ContainingClassName = "int",
                                                            MethodName = "Parse"
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution))
                .ReturnsAsync(It.IsAny<Solution>());
            _solutionLoadingStrategyMock
                .Setup(strategy => strategy.Load(It.IsAny<Solution>(), new List<IFactExtractor>()))
                .ReturnsAsync(solutionModelMock);

            var loadSolution = await _sut.LoadSolution(pathToSolution);

            Assert.NotNull(loadSolution);
            Assert.Equal(1, loadSolution.Projects.Count);
            Assert.Equal("Project1", loadSolution.Projects[0].Name);
            Assert.Equal(1, loadSolution.Projects[0].Namespaces.Count);

            var namespaceModel = loadSolution.Projects[0].Namespaces["Project1.Services"];
            Assert.Equal("Project1.Services", namespaceModel.Name);
            Assert.Equal(1, namespaceModel.ClassModels.Count);

            var classModel = namespaceModel.ClassModels[0];
            Assert.Equal("Project1.Services.CreateService", classModel.FullName);
            Assert.Equal("object", classModel.BaseClassFullName);
            Assert.Equal("Project1.Services.CreateService", classModel.FullName);
            Assert.Equal(1, classModel.BaseInterfaces.Count);
            Assert.Equal("IService", classModel.BaseInterfaces[0]);
            Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", classModel.FilePath);
            Assert.Empty(classModel.Fields);
            Assert.Empty(classModel.Metrics);
            Assert.Equal(2, classModel.Methods.Count);

            var methodA = classModel.Methods[0];
            Assert.Equal("A", methodA.Name);
            Assert.Equal("", methodA.Modifier);
            Assert.Equal("public", methodA.AccessModifier);
            Assert.Equal(1, methodA.ParameterTypes.Count);
            Assert.Equal("string", methodA.ParameterTypes[0].Type);
            Assert.Equal("", methodA.ParameterTypes[0].Modifier);
            Assert.Null(methodA.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", methodA.ReturnType);
            Assert.Equal("Project1.Services.CreateService", methodA.ContainingClassName);
            Assert.Equal(1, methodA.CalledMethods.Count);
            Assert.Equal("Project1.Services.CreateService", methodA.CalledMethods[0].ContainingClassName);
            Assert.Equal("Convert", methodA.CalledMethods[0].MethodName);

            var methodConvert = classModel.Methods[1];
            Assert.Equal("Convert", methodConvert.Name);
            Assert.Equal("static", methodConvert.Modifier);
            Assert.Equal("public", methodConvert.AccessModifier);
            Assert.Equal(1, methodConvert.ParameterTypes.Count);
            Assert.Equal("string", methodConvert.ParameterTypes[0].Type);
            Assert.Equal("", methodConvert.ParameterTypes[0].Modifier);
            Assert.Null(methodConvert.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", methodConvert.ReturnType);
            Assert.Equal("Project1.Services.CreateService", methodConvert.ContainingClassName);
            Assert.Equal(1, methodConvert.CalledMethods.Count);
            Assert.Equal("int", methodConvert.CalledMethods[0].ContainingClassName);
            Assert.Equal("Parse", methodConvert.CalledMethods[0].MethodName);
        }
    }
}