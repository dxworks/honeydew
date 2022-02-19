using System.Collections.Generic;
using HoneydewCore.Logging;
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
        private readonly Mock<ILogger> _ambiguousClassLoggerMock = new();
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly Mock<IProgressLoggerBar> _progressLoggerBarMock = new();

        public FullNameModelProcessorTests()
        {
            _sut = new FullNameModelProcessor(_loggerMock.Object, _ambiguousClassLoggerMock.Object,
                _progressLoggerMock.Object, false);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassNames_WhenGivenClassModels()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.Class1"
                    }
                }
            });

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Services.Class2"
                    }
                }
            });

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Controllers",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Controllers.Class3"
                    },
                    new ClassModel
                    {
                        Name = "Controllers.Class4"
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();

            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Domain.Data",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Domain.Data.Class5"
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel2);

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
                actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0].Name);
            Assert.Equal("Services.Class2",
                actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0].Name);
            Assert.Equal("Controllers.Class3",
                actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0].Name);
            Assert.Equal("Controllers.Class4",
                actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[1].Name);
            Assert.Equal("Domain.Data.Class5",
                actualRepositoryModel.Projects[1].CompilationUnits[0].ClassTypes[0].Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullClassName_WhenGivenClassWithInnerClass()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project1.Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Project1.Models.Class1"
                    },
                    new ClassModel
                    {
                        Name = "Project1.Models.Class1.InnerClass1"
                    },
                    new ClassModel
                    {
                        Name = "Project1.Models.Class1.InnerClass1.InnerClass2"
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

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

            var compilationUnitModel = actualRepositoryModel.Projects[0].CompilationUnits[0];
            Assert.Equal("Project1.Models.Class1", compilationUnitModel.ClassTypes[0].Name);
            Assert.Equal("Project1.Models.Class1.InnerClass1", compilationUnitModel.ClassTypes[1].Name);
            Assert.Equal("Project1.Models.Class1.InnerClass1.InnerClass2", compilationUnitModel.ClassTypes[2].Name);
        }

        [Fact(Skip = "Obsolete class")]
        public void Process_ShouldReturnTheFullClassName_WhenGivenClassMethodsWithParameters()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project1.Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Project1.Models.Class1",
                        ContainingClassName = "Project1.Models",
                    },
                    new ClassModel
                    {
                        Name = "Project1.Models.Class2",
                        ContainingClassName = "Project1.Models",
                        Constructors =
                        {
                            new ConstructorModel
                            {
                                ParameterTypes =
                                {
                                    new ParameterModel
                                    {
                                        Type = new EntityTypeModel
                                        {
                                            Name = "Class1"
                                        },
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
                                        Type = new EntityTypeModel
                                        {
                                            Name = "Class1",
                                        },
                                        Modifier = "",
                                        DefaultValue = ""
                                    }
                                }
                            }
                        }
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

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

            var compilationUnitModel = actualRepositoryModel.Projects[0].CompilationUnits[0];
            var classModel = (ClassModel)compilationUnitModel.ClassTypes[1];
            Assert.Equal("Project1.Models.Class1",
                classModel.Constructors[0].ParameterTypes[0].Type.Name);
            Assert.Equal("Project1.Models.Class1",
                classModel.Methods[0].ParameterTypes[0].Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullBaseClassNames_WhenGivenClassModels()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.Class1",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "object"
                                },
                                Kind = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Class2",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class1"
                                },
                                Kind = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Class3",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class1"
                                },
                                Kind = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.TheClass",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class3"
                                },
                                Kind = "class"
                            }
                        }
                    }
                }
            });

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models.Other",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.Other.Class1",
                        ContainingNamespaceName = "Models.Other",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class2"
                                },
                                Kind = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Other.Class2",
                        ContainingNamespaceName = "Models.Other",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Models.Class1"
                                },
                                Kind = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Other.Class3",
                        ContainingNamespaceName = "Models.Other",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "TheClass"
                                },
                                Kind = "class"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Other.SuperClass",
                        ContainingNamespaceName = "Models.Other",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class3"
                                },
                                Kind = "class"
                            }
                        }
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();

            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "MyNamespace",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.AClass",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "SuperClass"
                                },
                                Kind = "class"
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models.Other"
                            }
                        }
                    }
                }
            });

            projectModel1.FilePath = "path1";
            projectModel2.ProjectReferences = new List<string> { "path1" };

            repositoryModel.Projects.Add(projectModel2);
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

            var modelsNamespace = actualRepositoryModel.Projects[0].CompilationUnits[0];
            foreach (var classModel in modelsNamespace.ClassTypes)
            {
                Assert.Equal("class", classModel.BaseTypes[0].Kind);
            }

            Assert.Equal("object", modelsNamespace.ClassTypes[0].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Class1", modelsNamespace.ClassTypes[1].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Class1", modelsNamespace.ClassTypes[2].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Class3", modelsNamespace.ClassTypes[3].BaseTypes[0].Type.Name);


            var otherModelsNamespace = actualRepositoryModel.Projects[0].CompilationUnits[1];
            foreach (var classModel in otherModelsNamespace.ClassTypes)
            {
                Assert.Equal("class", classModel.BaseTypes[0].Kind);
            }

            Assert.Equal("Models.Other.Class2", otherModelsNamespace.ClassTypes[0].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Class1", otherModelsNamespace.ClassTypes[1].BaseTypes[0].Type.Name);
            Assert.Equal("Models.TheClass", otherModelsNamespace.ClassTypes[2].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Other.Class3", otherModelsNamespace.ClassTypes[3].BaseTypes[0].Type.Name);

            Assert.Equal("Models.Other.SuperClass",
                actualRepositoryModel.Projects[1].CompilationUnits[0].ClassTypes[0]
                    .BaseTypes[0].Type.Name);
        }

        [Fact]
        public void Process_ShouldReturnTheFullBaseInterfacesNames_WhenGivenClassModels()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();


            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models.Interfaces",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.Interfaces.IInterface1",
                        ContainingNamespaceName = "Models.Interfaces",
                    },
                    new ClassModel
                    {
                        Name = "Models.Interfaces.IInterface2",
                        ContainingNamespaceName = "Models.Interfaces",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "IInterface1"
                                },
                                Kind = "interface"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Interfaces.IInterface3",
                        ContainingNamespaceName = "Models.Interfaces",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Models.Interfaces.IInterface1"
                                },
                                Kind = "interface"
                            },
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "IInterface2"
                                },
                                Kind = "interface"
                            }
                        }
                    }
                }
            });

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.Class1",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "IInterface3"
                                },
                                Kind = "interface"
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models.Interfaces"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Class2",
                        ContainingNamespaceName = "",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "IInterface1"
                                },
                                Kind = "interface"
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models.Interfaces"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.Class3",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "IInterface1"
                                },
                                Kind = "interface"
                            },
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "IInterface2"
                                },
                                Kind = "interface"
                            },
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Models.Interfaces.IInterface3"
                                },
                                Kind = "interface"
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models.Interfaces"
                            }
                        }
                    },
                    new ClassModel
                    {
                        Name = "Models.TheClass",
                        ContainingNamespaceName = "Models",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Models.Interfaces.IInterface1"
                                },
                                Kind = "interface"
                            },
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "AInterface"
                                },
                                Kind = "interface"
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "MyNamespace"
                            }
                        }
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();

            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "MyNamespace",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyNamespace.AInterface",
                        ContainingNamespaceName = "MyNamespace",
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models"
                            }
                        }
                    }
                }
            });

            projectModel2.FilePath = "path2";
            projectModel1.ProjectReferences = new List<string> { "path2" };

            repositoryModel.Projects.Add(projectModel2);

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

            var modelInterfacesNamespace = actualRepositoryModel.Projects[0].CompilationUnits[0];
            Assert.Empty(modelInterfacesNamespace.ClassTypes[0].BaseTypes);

            foreach (var classModel in modelInterfacesNamespace.ClassTypes)
            {
                foreach (var baseType in classModel.BaseTypes)
                {
                    Assert.Equal("interface", baseType.Kind);
                }
            }

            Assert.Equal("Models.Interfaces.IInterface1",
                modelInterfacesNamespace.ClassTypes[1].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface1",
                modelInterfacesNamespace.ClassTypes[2].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface2",
                modelInterfacesNamespace.ClassTypes[2].BaseTypes[1].Type.Name);

            var modelsNamespace = actualRepositoryModel.Projects[0].CompilationUnits[1];

            foreach (var classModel in modelsNamespace.ClassTypes)
            {
                foreach (var baseType in classModel.BaseTypes)
                {
                    Assert.Equal("interface", baseType.Kind);
                }
            }

            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassTypes[0].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassTypes[1].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassTypes[2].BaseTypes[0].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface2", modelsNamespace.ClassTypes[2].BaseTypes[1].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassTypes[2].BaseTypes[2].Type.Name);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassTypes[3].BaseTypes[0].Type.Name);
            Assert.Equal("MyNamespace.AInterface", modelsNamespace.ClassTypes[3].BaseTypes[1].Type.Name);

            Assert.Empty(actualRepositoryModel.Projects[1].CompilationUnits[0].ClassTypes[0].BaseTypes);
        }

        [Fact]
        public void
            Process_ShouldHaveFindClassInOtherProject_WhenGivenAClassThatCouldNotBeenFoundInCurrentProject()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.SomeModel",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();
            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Services.Service",
                        ContainingNamespaceName = "Services",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "SomeModel",
                                    FullType = new GenericType
                                    {
                                        Name = "SomeModel"
                                    }
                                }
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models"
                            }
                        }
                    }
                }
            });

            projectModel1.FilePath = "path1";
            projectModel2.ProjectReferences = new List<string> { "path1" };

            repositoryModel.Projects.Add(projectModel2);
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
                ((ClassModel)actualRepositoryModel.Projects[1].CompilationUnits[0].ClassTypes[0]).Fields[0].Type
                .Name);
        }

        [Fact]
        public void
            Process_ShouldHaveFindClassInOtherSolution_WhenGivenAClassThatCouldNotBeenFoundInCurrentSolution()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel1 = new SolutionModel();
            var projectModel1 = new ProjectModel();


            projectModel1.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.SomeModel",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

            var projectModel2 = new ProjectModel();
            projectModel2.Add(new CompilationUnitModel
            {
                FilePath = "Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Services.Service",
                        ContainingNamespaceName = "Services",
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel2);

            var solutionModel2 = new SolutionModel();
            var projectModel3 = new ProjectModel();

            projectModel3.Add(new CompilationUnitModel
            {
                FilePath = "OtherSolutionNamespace",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "OtherSolutionNamespace.Controller",
                        ContainingNamespaceName = "OtherSolutionNamespace",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "SomeModel"
                                }
                            },
                        },
                        Methods =
                        {
                            new MethodModel
                            {
                                ReturnValue = new ReturnValueModel
                                {
                                    Type = new EntityTypeModel
                                    {
                                        Name = "Service"
                                    },
                                    Modifier = "ref"
                                }
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models"
                            },
                            new UsingModel
                            {
                                Name = "Services"
                            }
                        }
                    }
                }
            });

            projectModel1.FilePath = "path1";
            projectModel2.FilePath = "path2";
            projectModel3.ProjectReferences = new List<string> { "path1", "path2" };

            repositoryModel.Projects.Add(projectModel3);

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

            var classModel = (ClassModel)actualRepositoryModel.Projects[2].CompilationUnits[0].ClassTypes[0];
            Assert.Equal("Models.SomeModel", classModel.Fields[0].Type.Name);
            Assert.Equal("Services.Service", classModel.Methods[0].ReturnValue.Type.Name);
            Assert.Equal("ref", ((ReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
        }

        [Fact]
        public void
            Process_ShouldHavePropertiesWithAmbiguousTypesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameProject()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.AmbiguousClass",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            projectModel.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Services.AmbiguousClass",
                        ContainingNamespaceName = "Services",
                    }
                }
            });

            projectModel.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Controllers",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Controllers.AmbiguousClass",
                        ContainingNamespaceName = "Controllers",
                    }
                }
            });

            projectModel.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "SomeNamespace",
                ClassTypes =
                {
                    new ClassModel
                    {
                        FilePath = "SomePath/SomeClass.cs",
                        Name = "SomeNamespace.SomeClass",
                        ContainingNamespaceName = "SomeNamespace",
                        BaseTypes = new List<IBaseType>
                        {
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "AmbiguousClass"
                                },
                                Kind = "class"
                            },
                            new BaseTypeModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "AmbiguousClass"
                                },
                                Kind = "interface"
                            }
                        },
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "AmbiguousClass"
                                }
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
                                        Type = new EntityTypeModel
                                        {
                                            Name = "AmbiguousClass"
                                        }
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
                                        Type = new EntityTypeModel
                                        {
                                            Name = "AmbiguousClass"
                                        }
                                    }
                                },
                                CalledMethods =
                                {
                                    new MethodCallModel
                                    {
                                        LocationClassName = "AmbiguousClass",
                                        DefinitionClassName = "AmbiguousClass",
                                        ParameterTypes =
                                        {
                                            new ParameterModel
                                            {
                                                Type = new EntityTypeModel
                                                {
                                                    Name = "AmbiguousClass"
                                                }
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
                                ExtractorName = "ParametersExtractor",
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

            repositoryModel.Projects.Add(projectModel);

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
                actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0].Name);
            Assert.Equal("Services.AmbiguousClass",
                actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0].Name);
            Assert.Equal("Controllers.AmbiguousClass",
                actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0].Name);

            var someClassModel =
                (ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[3].ClassTypes[0];
            Assert.Equal("SomeNamespace.SomeClass",
                someClassModel.Name);

            Assert.Equal(2, someClassModel.BaseTypes.Count);
            foreach (var baseType in someClassModel.BaseTypes)
            {
                Assert.Equal("AmbiguousClass", baseType.Type.Name);
            }

            Assert.Equal("class", someClassModel.BaseTypes[0].Kind);
            Assert.Equal("interface", someClassModel.BaseTypes[1].Kind);
            Assert.Equal("AmbiguousClass", someClassModel.Constructors[0].ParameterTypes[0].Type.Name);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].LocationClassName);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].DefinitionClassName);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("AmbiguousClass", someClassModel.Fields[0].Type.Name);
            var metricDependencies = ((Dictionary<string, int>)someClassModel.Metrics[0].Value);
            Assert.Single(metricDependencies);
            Assert.True(metricDependencies.ContainsKey("AmbiguousClass"));
        }

        [Fact]
        public void
            Process_ShouldHavePropertiesWithAmbiguousNamesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameSolution()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project1.Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel2 = new ProjectModel();
            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project2.Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel3 = new ProjectModel();
            projectModel3.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Controllers",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyController",
                        FilePath = "SomePath/MyController.cs",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "MyService"
                                }
                            }
                        }
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);
            repositoryModel.Projects.Add(projectModel2);
            repositoryModel.Projects.Add(projectModel3);

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
                ((ClassModel)actualRepositoryModel.Projects[2].CompilationUnits[0].ClassTypes[0]).Fields[0].Type
                .Name);
        }

        [Fact]
        public void
            Process_ShouldHavePropertiesWithAmbiguousNamesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameRepository()
        {
            var repositoryModel = new RepositoryModel();

            var solutionModel1 = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project1.Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Project1.Services.MyService"
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

            var solutionModel2 = new SolutionModel();
            var projectModel2 = new ProjectModel();
            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project2.Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Project2.Services.MyService"
                    }
                }
            });


            repositoryModel.Projects.Add(projectModel2);

            var solutionModel3 = new SolutionModel();
            var projectModel3 = new ProjectModel();
            projectModel3.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "NamespaceName",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "NamespaceName.MyController",
                        FilePath = "SomePath/MyController.cs",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "MyService"
                                }
                            }
                        }
                    }
                }
            });
            repositoryModel.Projects.Add(projectModel3);

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
                ((ClassModel)actualRepositoryModel.Projects[2].CompilationUnits[0].ClassTypes[0]).Fields[0].Type
                .Name);
        }

        [Fact]
        public void
            Process_ShouldHaveClassNameTheSame_WhenClassNameDoesNotExistInRepository()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel1 = new SolutionModel();
            var projectModel1 = new ProjectModel();

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project1.Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel2 = new ProjectModel();
            projectModel2.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Project2.Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyService"
                    }
                }
            });

            var projectModel3 = new ProjectModel();
            projectModel3.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Controllers",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "MyController",
                        Fields =
                        {
                            new FieldModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "OutOfRepositoryClass"
                                }
                            }
                        }
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);
            repositoryModel.Projects.Add(projectModel2);


            var solutionModel2 = new SolutionModel();
            repositoryModel.Projects.Add(projectModel3);
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
                ((ClassModel)actualRepositoryModel.Projects[2].CompilationUnits[0].ClassTypes[0]).Fields[0].Type
                .Name);

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

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Models",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Models.Class1",
                        ContainingNamespaceName = "Models",
                        Properties =
                        {
                            new PropertyModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "int"
                                }
                            }
                        }
                    }
                }
            });

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Services",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Services.Class2",
                        ContainingNamespaceName = "Services",
                        Properties =
                        {
                            new PropertyModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class1"
                                }
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Models"
                            }
                        }
                    }
                }
            });

            projectModel1.CompilationUnits.Add(new CompilationUnitModel
            {
                FilePath = "Controllers",
                ClassTypes =
                {
                    new ClassModel
                    {
                        Name = "Controllers.Class4",
                        ContainingNamespaceName = "Controllers",
                        Properties =
                        {
                            new PropertyModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Class2"
                                }
                            },
                            new PropertyModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "string"
                                }
                            },
                            new PropertyModel
                            {
                                Type = new EntityTypeModel
                                {
                                    Name = "Namespace.RandomClassClass"
                                }
                            }
                        },
                        Imports = new List<IImportType>
                        {
                            new UsingModel
                            {
                                Name = "Services"
                            }
                        }
                    }
                }
            });

            repositoryModel.Projects.Add(projectModel1);

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

            Assert.Equal("int",
                ((ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[0].ClassTypes[0]).Properties[0]
                .Type.Name);
            Assert.Equal("Models.Class1",
                ((ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[1].ClassTypes[0]).Properties[0]
                .Type.Name);
            Assert.Equal("Services.Class2",
                ((ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0]).Properties[0]
                .Type.Name);
            Assert.Equal("string",
                ((ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0]).Properties[1]
                .Type.Name);
            Assert.Equal("Namespace.RandomClassClass",
                ((ClassModel)actualRepositoryModel.Projects[0].CompilationUnits[2].ClassTypes[0]).Properties[2]
                .Type.Name);
        }
    }
}
