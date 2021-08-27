using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead;
using HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.RepositoryLoading
{
    public class SolutionFileLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<ISolutionProvider> _solutionProviderMock = new();
        private readonly Mock<ISolutionLoadingStrategy> _solutionLoadingStrategyMock = new();
        private readonly Mock<ILogger> _progressLoggerMock = new();
        private readonly Mock<IFactExtractorCreator> _extractorCreatorMock = new();

        public SolutionFileLoaderTests()
        {
            _sut = new SolutionFileLoader(_progressLoggerMock.Object, _extractorCreatorMock.Object,
                _solutionProviderMock.Object,
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
                .Setup(strategy => strategy.Load(It.IsAny<Solution>(), _extractorCreatorMock.Object))
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
                            new NamespaceModel
                            {
                                Name = "Project1.Services",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "CreateService",
                                        FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                        BaseTypes = new List<IBaseType>
                                        {
                                            new BaseTypeModel
                                            {
                                                Type = new EntityTypeModel
                                                {
                                                    Name = "object",
                                                },
                                                Kind = "class"
                                            },
                                            new BaseTypeModel
                                            {
                                                Type = new EntityTypeModel
                                                {
                                                    Name = "IService",
                                                },
                                                Kind = "interface"
                                            }
                                        },
                                        Methods = new List<IMethodType>
                                        {
                                            new MethodModel
                                            {
                                                Name = "A",
                                                Modifier = "",
                                                AccessModifier = "public",
                                                ParameterTypes =
                                                {
                                                    new ParameterModel
                                                    {
                                                        Type = new EntityTypeModel
                                                        {
                                                            Name = "string"
                                                        }
                                                    }
                                                },
                                                ReturnValue = new ReturnValueModel
                                                {
                                                    Type = new EntityTypeModel
                                                    {
                                                        Name = "int"
                                                    }
                                                },
                                                ContainingTypeName = "Project1.Services.CreateService",
                                                CalledMethods =
                                                {
                                                    new MethodCallModel
                                                    {
                                                        ContainingTypeName = "Project1.Services.CreateService",
                                                        Name = "Convert"
                                                    }
                                                }
                                            },
                                            new MethodModel
                                            {
                                                Name = "Convert",
                                                Modifier = "static",
                                                AccessModifier = "public",
                                                ParameterTypes =
                                                {
                                                    new ParameterModel
                                                    {
                                                        Type = new EntityTypeModel
                                                        {
                                                            Name = "string"
                                                        }
                                                    }
                                                },
                                                ReturnValue = new ReturnValueModel
                                                {
                                                    Type = new EntityTypeModel
                                                    {
                                                        Name = "int"
                                                    }
                                                },
                                                ContainingTypeName = "Project1.Services.CreateService",
                                                CalledMethods =
                                                {
                                                    new MethodCallModel
                                                    {
                                                        ContainingTypeName = "int",
                                                        Name = "Parse"
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
                .Setup(strategy => strategy.Load(It.IsAny<Solution>(), _extractorCreatorMock.Object))
                .ReturnsAsync(solutionModelMock);

            var loadSolution = await _sut.LoadSolution(pathToSolution);

            Assert.NotNull(loadSolution);
            Assert.Equal(1, loadSolution.Projects.Count);
            Assert.Equal("Project1", loadSolution.Projects[0].Name);
            Assert.Equal(1, loadSolution.Projects[0].Namespaces.Count);

            var namespaceModel = loadSolution.Projects[0].Namespaces[0];
            Assert.Equal("Project1.Services", namespaceModel.Name);
            Assert.Equal(1, namespaceModel.ClassModels.Count);

            var classModel = (ClassModel)namespaceModel.ClassModels[0];
            Assert.Equal("CreateService", classModel.Name);
            Assert.Equal(2, classModel.BaseTypes.Count);

            Assert.Equal("object", classModel.BaseTypes[0].Type.Name);
            Assert.Equal("class", classModel.BaseTypes[0].Kind);
            Assert.Equal("IService", classModel.BaseTypes[1].Type.Name);
            Assert.Equal("interface", classModel.BaseTypes[1].Kind);
            Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", classModel.FilePath);
            Assert.Empty(classModel.Fields);
            Assert.Empty(classModel.Metrics);
            Assert.Equal(2, classModel.Methods.Count);

            var methodA = classModel.Methods[0];
            Assert.Equal("A", methodA.Name);
            Assert.Equal("", methodA.Modifier);
            Assert.Equal("public", methodA.AccessModifier);
            Assert.Equal(1, methodA.ParameterTypes.Count);
            var methodAParameter = (ParameterModel)methodA.ParameterTypes[0];
            Assert.Equal("string", methodAParameter.Type.Name);
            Assert.Equal("", methodAParameter.Modifier);
            Assert.Null(methodAParameter.DefaultValue);
            Assert.Equal("int", methodA.ReturnValue.Type.Name);
            Assert.Equal("Project1.Services.CreateService", methodA.ContainingTypeName);
            Assert.Equal(1, methodA.CalledMethods.Count);
            Assert.Equal("Project1.Services.CreateService", methodA.CalledMethods[0].ContainingTypeName);
            Assert.Equal("Convert", methodA.CalledMethods[0].Name);

            var methodConvert = classModel.Methods[1];
            Assert.Equal("Convert", methodConvert.Name);
            Assert.Equal("static", methodConvert.Modifier);
            Assert.Equal("public", methodConvert.AccessModifier);
            Assert.Equal(1, methodConvert.ParameterTypes.Count);
            var methodConvertParameter = (ParameterModel)methodConvert.ParameterTypes[0];
            Assert.Equal("string", methodConvertParameter.Type.Name);
            Assert.Equal("", methodConvertParameter.Modifier);
            Assert.Null(methodConvertParameter.DefaultValue);
            Assert.Equal("int", methodConvert.ReturnValue.Type.Name);
            Assert.Equal("Project1.Services.CreateService", methodConvert.ContainingTypeName);
            Assert.Equal(1, methodConvert.CalledMethods.Count);
            Assert.Equal("int", methodConvert.CalledMethods[0].ContainingTypeName);
            Assert.Equal("Parse", methodConvert.CalledMethods[0].Name);
        }
    }
}
