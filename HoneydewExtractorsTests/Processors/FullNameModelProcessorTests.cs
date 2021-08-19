using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.Processors
{
    public class FullNameModelProcessorTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly Mock<IProgressLoggerBar> _progressLoggerBarMock = new();

        public FullNameModelProcessorTests()
        {
            _sut = new FullNameModelProcessor(_loggerMock.Object, _progressLoggerMock.Object);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassNames_WhenGivenClassModels()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class1"
                    }
                }
            });

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class2"
                    }
                }
            });

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Controllers",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Controllers.Class3"
                    },
                    new ClassModel
                    {
                        Name = "Class4"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();

            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "Domain.Data",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class5"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel2);

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(5, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(5, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(5, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("Models.Class1",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].Name);
            Assert.Equal("Services.Class2",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0].Name);
            Assert.Equal("Controllers.Class3",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].Name);
            Assert.Equal("Controllers.Class4",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[1].Name);
            Assert.Equal("Domain.Data.Class5",
                actualRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassName_WhenGivenClassWithInnerClass()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Project1.Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class1"
                    },
                    new ClassModel
                    {
                        Name = "Class1.InnerClass1"
                    },
                    new ClassModel
                    {
                        Name = "Class1.InnerClass1.InnerClass2"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            var namespaceModel = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0];
            Assert.Equal("Project1.Models.Class1", namespaceModel.ClassModels[0].Name);
            Assert.Equal("Project1.Models.Class1.InnerClass1", namespaceModel.ClassModels[1].Name);
            Assert.Equal("Project1.Models.Class1.InnerClass1.InnerClass2", namespaceModel.ClassModels[2].Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassName_WhenGivenClassMethodsWithParameters()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Project1.Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class1"
                    },
                    new ClassModel
                    {
                        Name = "Class2",
                        Constructors =
                        {
                            new ConstructorModel
                            {
                                ParameterTypes =
                                {
                                    new ParameterModel
                                    {
                                        Name = "Class1",
                                        Modifier = "",
                                        DefaultValue = ""
                                    }
                                }
                            }
                        },
                        Methods =
                        {
                            new MethodModel
                            {
                                ParameterTypes =
                                {
                                    new ParameterModel
                                    {
                                        Name = "Class1",
                                        Modifier = "",
                                        DefaultValue = ""
                                    }
                                }
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(2, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(2, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(2, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            var namespaceModel = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0];
            Assert.Equal("Project1.Models.Class1",
                namespaceModel.ClassModels[1].Constructors[0].ParameterTypes[0].Name);
            Assert.Equal("Project1.Models.Class1", namespaceModel.ClassModels[1].Methods[0].ParameterTypes[0].Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullBaseClassNames_WhenGivenClassModels()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Models.Class1",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "object",
                                ClassType = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Class2",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Class1",
                                ClassType = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Class3",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Class1",
                                ClassType = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.TheClass",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Class3",
                                ClassType = "class"
                            }
                        }
                    }
                }
            });

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models.Other",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Models.Other.Class1",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Class2",
                                ClassType = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Other.Class2",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Models.Class1",
                                ClassType = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Other.Class3",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "TheClass",
                                ClassType = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Other.SuperClass",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Class3",
                                ClassType = "class"
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();

            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "MyNamespace",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Models.AClass",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "SuperClass",
                                ClassType = "class"
                            }
                        }
                    }
                }
            });

            projectModel1.FilePath = "path1";
            projectModel2.ProjectReferences = new List<string> { "path1" };

            solutionModel.Projects.Add(projectModel2);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(9, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(9, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(9, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            var modelsNamespace = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0];
            foreach (var classModel in modelsNamespace.ClassModels)
            {
                Assert.Equal("class", classModel.BaseTypes[0].ClassType);
            }

            Assert.Equal("System.Object", modelsNamespace.ClassModels[0].BaseTypes[0].Name);
            Assert.Equal("Models.Class1", modelsNamespace.ClassModels[1].BaseTypes[0].Name);
            Assert.Equal("Models.Class1", modelsNamespace.ClassModels[2].BaseTypes[0].Name);
            Assert.Equal("Models.Class3", modelsNamespace.ClassModels[3].BaseTypes[0].Name);


            var otherModelsNamespace = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1];
            foreach (var classModel in otherModelsNamespace.ClassModels)
            {
                Assert.Equal("class", classModel.BaseTypes[0].ClassType);
            }

            Assert.Equal("Models.Other.Class2", otherModelsNamespace.ClassModels[0].BaseTypes[0].Name);
            Assert.Equal("Models.Class1", otherModelsNamespace.ClassModels[1].BaseTypes[0].Name);
            Assert.Equal("Models.TheClass", otherModelsNamespace.ClassModels[2].BaseTypes[0].Name);
            Assert.Equal("Models.Other.Class3", otherModelsNamespace.ClassModels[3].BaseTypes[0].Name);

            Assert.Equal("Models.Other.SuperClass",
                actualRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0]
                    .BaseTypes[0].Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullBaseInterfacesNames_WhenGivenClassModels()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();


            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models.Interfaces",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "IInterface1"
                    },
                    new ClassModel
                    {
                        Name = "Models.Interfaces.IInterface2",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "IInterface1",
                                ClassType = "interface"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Interfaces.IInterface3",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Models.Interfaces.IInterface1",
                                ClassType = "interface"
                            },
                            new BaseTypeModel
                            {
                                Name = "IInterface2",
                                ClassType = "interface"
                            }
                        }
                    }
                }
            });

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Models.Class1",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "IInterface3",
                                ClassType = "interface"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Class2",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "IInterface1",
                                ClassType = "interface"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Class3",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "IInterface1",
                                ClassType = "interface"
                            },
                            new BaseTypeModel
                            {
                                Name = "IInterface2",
                                ClassType = "interface"
                            },
                            new BaseTypeModel
                            {
                                Name = "Models.Interfaces.IInterface3",
                                ClassType = "interface"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.TheClass",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "Models.Interfaces.IInterface1",
                                ClassType = "interface"
                            },
                            new BaseTypeModel
                            {
                                Name = "AInterface",
                                ClassType = "interface"
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();

            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "MyNamespace",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "AInterface",
                    }
                }
            });

            projectModel2.FilePath = "path2";
            projectModel1.ProjectReferences = new List<string> { "path2" };

            solutionModel.Projects.Add(projectModel2);

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(8, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(8, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(8, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            var modelInterfacesNamespace = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0];
            Assert.Empty(modelInterfacesNamespace.ClassModels[0].BaseTypes);

            foreach (var classModel in modelInterfacesNamespace.ClassModels)
            {
                foreach (var baseType in classModel.BaseTypes)
                {
                    Assert.Equal("interface", baseType.ClassType);
                }
            }

            Assert.Equal("Models.Interfaces.IInterface1", modelInterfacesNamespace.ClassModels[1].BaseTypes[0].Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelInterfacesNamespace.ClassModels[2].BaseTypes[0].Name);
            Assert.Equal("Models.Interfaces.IInterface2", modelInterfacesNamespace.ClassModels[2].BaseTypes[1].Name);

            var modelsNamespace = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1];

            foreach (var classModel in modelsNamespace.ClassModels)
            {
                foreach (var baseType in classModel.BaseTypes)
                {
                    Assert.Equal("interface", baseType.ClassType);
                }
            }

            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassModels[0].BaseTypes[0].Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[1].BaseTypes[0].Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[2].BaseTypes[0].Name);
            Assert.Equal("Models.Interfaces.IInterface2", modelsNamespace.ClassModels[2].BaseTypes[1].Name);
            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassModels[2].BaseTypes[2].Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[3].BaseTypes[0].Name);
            Assert.Equal("MyNamespace.AInterface", modelsNamespace.ClassModels[3].BaseTypes[1].Name);

            Assert.Empty(actualRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].BaseTypes);
        }

        [Fact]
        public void Process_ShouldReturnTheFullNamesOfContainingClassNameOfMethods_WhenGivenClassModelsWithMethods()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();


            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Project1.Models.Classes",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class1",
                        Methods =
                        {
                            new MethodModel
                            {
                                ContainingTypeName = "Class1"
                            },
                            new MethodModel
                            {
                                ContainingTypeName = "Project1.Models.Classes.Class1"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Project1.Models.Classes.Class2",
                        Methods =
                        {
                            new MethodModel
                            {
                                ContainingTypeName = "Class2",
                            }
                        },
                        Constructors =
                        {
                            new ConstructorModel
                            {
                                ContainingTypeName = "Class2"
                            },
                            new ConstructorModel
                            {
                                ContainingTypeName = "Project1.Models.Classes.Class2"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Project1.Models.Classes.Class3",
                        Constructors =
                        {
                            new ConstructorModel
                            {
                                ContainingTypeName = "Class3",
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);


            var actualRepositoryModel = _sut.Process(repositoryModel);

            var modelInterfacesNamespace =
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0];

            Assert.Empty(modelInterfacesNamespace.ClassModels[0].Constructors);
            Assert.Equal(2, modelInterfacesNamespace.ClassModels[0].Methods.Count);
            Assert.Equal("Project1.Models.Classes.Class1",
                modelInterfacesNamespace.ClassModels[0].Methods[0].ContainingTypeName);
            Assert.Equal("Project1.Models.Classes.Class1",
                modelInterfacesNamespace.ClassModels[0].Methods[1].ContainingTypeName);

            Assert.Equal(1, modelInterfacesNamespace.ClassModels[1].Methods.Count);
            Assert.Equal("Project1.Models.Classes.Class2",
                modelInterfacesNamespace.ClassModels[1].Methods[0].ContainingTypeName);
            Assert.Equal(2, modelInterfacesNamespace.ClassModels[1].Constructors.Count);
            Assert.Equal("Project1.Models.Classes.Class2",
                modelInterfacesNamespace.ClassModels[1].Constructors[0].ContainingTypeName);
            Assert.Equal("Project1.Models.Classes.Class2",
                modelInterfacesNamespace.ClassModels[1].Constructors[1].ContainingTypeName);

            Assert.Empty(modelInterfacesNamespace.ClassModels[2].Methods);
            Assert.Equal(1, modelInterfacesNamespace.ClassModels[2].Constructors.Count);
            Assert.Equal("Project1.Models.Classes.Class3",
                modelInterfacesNamespace.ClassModels[2].Constructors[0].ContainingTypeName);
        }

        [Fact]
        public void
            Process_ShouldHaveFindClassInOtherProject_WhenGivenAClassThatCouldNotBeenFoundInCurrentProject()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "SomeModel"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();
            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Service",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = "SomeModel"
                            }
                        }
                    }
                }
            });

            projectModel1.FilePath = "path1";
            projectModel2.ProjectReferences = new List<string> { "path1" };

            solutionModel.Projects.Add(projectModel2);
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(2, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(2, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(2, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("Models.SomeModel",
                actualRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].Fields[0].Type);
        }

        [Fact]
        public void
            Process_ShouldHaveFindClassInOtherSolution_WhenGivenAClassThatCouldNotBeenFoundInCurrentSolution()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel1 = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "SomeModel"
                    }
                }
            });

            solutionModel1.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();
            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Service",
                    }
                }
            });

            solutionModel1.Projects.Add(projectModel2);

            var solutionModel2 = new SolutionModel();
            var projectModel3 = new ProjectModel();

            projectModel3.Namespaces.Add(new NamespaceModel
            {
                Name = "OtherSolutionNamespace",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Controller",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = "SomeModel"
                            },
                        },
                        Methods =
                        {
                            new MethodModel
                            {
                                ReturnType = new ReturnTypeModel
                                {
                                    Name = "Service",
                                    Modifier = "ref"
                                }
                            }
                        }
                    }
                }
            });

            projectModel1.FilePath = "path1";
            projectModel2.FilePath = "path2";
            projectModel3.ProjectReferences = new List<string> { "path1", "path2" };

            solutionModel2.Projects.Add(projectModel3);

            repositoryModel.Solutions.Add(solutionModel1);
            repositoryModel.Solutions.Add(solutionModel2);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("Models.SomeModel",
                actualRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0]
                    .Fields[0].Type);
            Assert.Equal("Services.Service",
                actualRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0]
                    .Methods[0].ReturnType.Name);
            Assert.Equal("ref",
                ((ReturnTypeModel)actualRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0].Methods[0]
                    .ReturnType).Modifier);
        }

        [Fact]
        public void
            Process_ShouldHavePropertiesWithAmbiguousTypesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameProject()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "AmbiguousClass"
                    }
                }
            });

            projectModel.Namespaces.Add(new NamespaceModel
            {
                Name = "Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "AmbiguousClass"
                    }
                }
            });

            projectModel.Namespaces.Add(new NamespaceModel
            {
                Name = "Controllers",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "AmbiguousClass"
                    }
                }
            });

            projectModel.Namespaces.Add(new NamespaceModel
            {
                Name = "SomeNamespace",
                ClassModels =
                {
                    new ClassModel
                    {
                        FilePath = "SomePath/SomeClass.cs",
                        Name = "SomeClass",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Name = "AmbiguousClass",
                                ClassType = "class"
                            },
                            new BaseTypeModel
                            {
                                Name = "AmbiguousClass",
                                ClassType = "interface"
                            }
                        },
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = "AmbiguousClass"
                            }
                        },
                        Constructors =
                        {
                            new ConstructorModel
                            {
                                ParameterTypes =
                                {
                                    new ParameterModel
                                    {
                                        Name = "AmbiguousClass"
                                    }
                                }
                            }
                        },
                        Methods =
                        {
                            new MethodModel
                            {
                                ParameterTypes =
                                {
                                    new ParameterModel
                                    {
                                        Name = "AmbiguousClass"
                                    }
                                },
                                CalledMethods =
                                {
                                    new MethodCallModel
                                    {
                                        ContainingTypeName = "AmbiguousClass",
                                        ParameterTypes =
                                        {
                                            new ParameterModel
                                            {
                                                Name = "AmbiguousClass"
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        Metrics =
                        {
                            new MetricModel
                            {
                                ExtractorName = typeof(CSharpParameterRelationMetric).FullName,
                                ValueType = typeof(Dictionary<string, int>).FullName,
                                Value = new Dictionary<string, int>()
                                {
                                    { "AmbiguousClass", 2 }
                                },
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel);

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(4, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(4, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(4, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("Models.AmbiguousClass",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].Name);
            Assert.Equal("Services.AmbiguousClass",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0].Name);
            Assert.Equal("Controllers.AmbiguousClass",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].Name);

            var someClassModel =
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[3].ClassModels[0];
            Assert.Equal("SomeNamespace.SomeClass",
                someClassModel.Name);

            Assert.Equal(2, someClassModel.BaseTypes.Count);
            foreach (var baseType in someClassModel.BaseTypes)
            {
                Assert.Equal("AmbiguousClass", baseType.Name);
            }

            Assert.Equal("class", someClassModel.BaseTypes[0].ClassType);
            Assert.Equal("interface", someClassModel.BaseTypes[1].ClassType);
            Assert.Equal("AmbiguousClass", someClassModel.Constructors[0].ParameterTypes[0].Name);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].ParameterTypes[0].Name);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].ParameterTypes[0].Name);
            Assert.Equal("AmbiguousClass", someClassModel.Fields[0].Type);
            var metricDependencies = ((Dictionary<string, int>)someClassModel.Metrics[0].Value);
            Assert.Single(metricDependencies);
            Assert.True(metricDependencies.ContainsKey("AmbiguousClass"));

            _loggerMock.Verify(
                logger => logger.Log("Multiple full names found for AmbiguousClass in SomePath/SomeClass.cs :",
                    LogLevels.Warning),
                Times.Once);
            _loggerMock.Verify(logger => logger.Log("Models.AmbiguousClass", LogLevels.Information));
            _loggerMock.Verify(logger => logger.Log("Services.AmbiguousClass", LogLevels.Information));
        }

        [Fact]
        public void
            Process_ShouldHavePropertiesWithAmbiguousNamesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameSolution()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Project1.Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel2 = new ProjectModel();
            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "Project2.Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel3 = new ProjectModel();
            projectModel3.Namespaces.Add(new NamespaceModel
            {
                Name = "Controllers",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyController",
                        FilePath = "SomePath/MyController.cs",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = "MyService"
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);
            solutionModel.Projects.Add(projectModel2);
            solutionModel.Projects.Add(projectModel3);

            projectModel1.FilePath = "path1";
            projectModel2.FilePath = "path2";
            projectModel3.ProjectReferences = new List<string> { "path1", "path2" };

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("MyService",
                actualRepositoryModel.Solutions[0].Projects[2].Namespaces[0].ClassModels[0].Fields[0].Type);

            _loggerMock.Verify(
                logger => logger.Log("Multiple full names found for MyService in SomePath/MyController.cs :",
                    LogLevels.Warning),
                Times.Once);
            _loggerMock.Verify(logger => logger.Log("Project1.Services.MyService", LogLevels.Information));
            _loggerMock.Verify(logger => logger.Log("Project2.Services.MyService", LogLevels.Information));
        }

        [Fact]
        public void
            Process_ShouldHavePropertiesWithAmbiguousNamesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameRepository()
        {
            var repositoryModel = new RepositoryModel();

            var solutionModel1 = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Project1.Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            solutionModel1.Projects.Add(projectModel1);

            var solutionModel2 = new SolutionModel();
            var projectModel2 = new ProjectModel();
            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "Project2.Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });


            solutionModel2.Projects.Add(projectModel2);

            var solutionModel3 = new SolutionModel();
            var projectModel3 = new ProjectModel();
            projectModel3.Namespaces.Add(new NamespaceModel
            {
                Name = "NamespaceName",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyController",
                        FilePath = "SomePath/MyController.cs",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = "MyService"
                            }
                        }
                    }
                }
            });
            solutionModel3.Projects.Add(projectModel3);

            projectModel1.FilePath = "path1";
            projectModel2.FilePath = "path2";
            projectModel3.ProjectReferences = new List<string> { "path1", "path2" };

            repositoryModel.Solutions.Add(solutionModel1);
            repositoryModel.Solutions.Add(solutionModel2);
            repositoryModel.Solutions.Add(solutionModel3);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("MyService",
                actualRepositoryModel.Solutions[2].Projects[0].Namespaces[0].ClassModels[0].Fields[0].Type);

            _loggerMock.Verify(
                logger => logger.Log("Multiple full names found for MyService in SomePath/MyController.cs :",
                    LogLevels.Warning),
                Times.Once);
            _loggerMock.Verify(logger => logger.Log("Project1.Services.MyService", LogLevels.Information));
            _loggerMock.Verify(logger => logger.Log("Project2.Services.MyService", LogLevels.Information));
        }

        [Fact]
        public void
            Process_ShouldHaveClassNameTheSame_WhenClassNameDoesNotExistInRepository()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel1 = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Project1.Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel2 = new ProjectModel();
            projectModel2.Namespaces.Add(new NamespaceModel
            {
                Name = "Project2.Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel3 = new ProjectModel();
            projectModel3.Namespaces.Add(new NamespaceModel
            {
                Name = "Controllers",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "MyController",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = "OutOfRepositoryClass"
                            }
                        }
                    }
                }
            });

            solutionModel1.Projects.Add(projectModel1);
            solutionModel1.Projects.Add(projectModel2);


            var solutionModel2 = new SolutionModel();
            solutionModel2.Projects.Add(projectModel3);
            repositoryModel.Solutions.Add(solutionModel1);
            repositoryModel.Solutions.Add(solutionModel2);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("OutOfRepositoryClass",
                actualRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0].Fields[0].Type);

            _loggerMock.Verify(
                logger => logger.Log("Multiple full names found for MyService: ", LogLevels.Warning),
                Times.Never);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassNames_WhenGivenClassModelsWithProperties()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Models",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class1",
                        Properties =
                        {
                            new PropertyModel
                            {
                                Type = "int"
                            }
                        }
                    }
                }
            });

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Services",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class2",
                        Properties =
                        {
                            new PropertyModel
                            {
                                Type = "Class1"
                            }
                        }
                    }
                }
            });

            projectModel1.Namespaces.Add(new NamespaceModel
            {
                Name = "Controllers",
                ClassModels =
                {
                    new ClassModel
                    {
                        Name = "Class4",
                        Properties =
                        {
                            new PropertyModel
                            {
                                Type = "Class2"
                            },
                            new PropertyModel
                            {
                                Type = "string"
                            },
                            new PropertyModel
                            {
                                Type = "Namespace.RandomClassClass"
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(3, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(3, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            Assert.Equal("System.Int32",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].Properties[0].Type);
            Assert.Equal("Models.Class1",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0].Properties[0].Type);
            Assert.Equal("Services.Class2",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].Properties[0].Type);
            Assert.Equal("System.String",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].Properties[1].Type);
            Assert.Equal("Namespace.RandomClassClass",
                actualRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].Properties[2].Type);
        }
    }
}
