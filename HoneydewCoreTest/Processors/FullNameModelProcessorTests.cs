using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewModels;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class FullNameModelProcessorTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();

        public FullNameModelProcessorTests()
        {
            _sut = new FullNameModelProcessor(_progressLoggerMock.Object);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassNames_WhenGivenClassModels()
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
                        FullName = "Class1"
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
                        FullName = "Class2"
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
                        FullName = "Controllers.Class3"
                    },
                    new ClassModel
                    {
                        FullName = "Class4"
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
                        FullName = "Class5"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel2);

            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("Models.Class1",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].FullName);
            Assert.Equal("Services.Class2",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0].FullName);
            Assert.Equal("Controllers.Class3",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].FullName);
            Assert.Equal("Controllers.Class4",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[1].FullName);
            Assert.Equal("Domain.Data.Class5",
                actualSolutionModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].FullName);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassName_WhenGivenClassWithInnerClass()
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
                        FullName = "Class1"
                    },
                    new ClassModel
                    {
                        FullName = "Class1.InnerClass1"
                    },
                    new ClassModel
                    {
                        FullName = "Class1.InnerClass1.InnerClass2"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);

            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            var namespaceModel = actualSolutionModel.Solutions[0].Projects[0].Namespaces[0];
            Assert.Equal("Project1.Models.Class1", namespaceModel.ClassModels[0].FullName);
            Assert.Equal("Project1.Models.Class1.InnerClass1", namespaceModel.ClassModels[1].FullName);
            Assert.Equal("Project1.Models.Class1.InnerClass1.InnerClass2", namespaceModel.ClassModels[2].FullName);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullBaseClassNames_WhenGivenClassModels()
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
                        FullName = "Models.Class1",
                        BaseClassFullName = "object"
                    },
                    new ClassModel
                    {
                        FullName = "Models.Class2",
                        BaseClassFullName = "Class1"
                    },
                    new ClassModel
                    {
                        FullName = "Models.Class3",
                        BaseClassFullName = "Class1"
                    },
                    new ClassModel
                    {
                        FullName = "Models.TheClass",
                        BaseClassFullName = "Class3"
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
                        FullName = "Models.Other.Class1",
                        BaseClassFullName = "Class2"
                    },
                    new ClassModel
                    {
                        FullName = "Models.Other.Class2",
                        BaseClassFullName = "Models.Class1"
                    },
                    new ClassModel
                    {
                        FullName = "Models.Other.Class3",
                        BaseClassFullName = "TheClass"
                    },
                    new ClassModel
                    {
                        FullName = "Models.Other.SuperClass",
                        BaseClassFullName = "Class3"
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
                        FullName = "Models.AClass",
                        BaseClassFullName = "SuperClass"
                    }
                }
            });

            solutionModel.Projects.Add(projectModel2);
            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            var modelsNamespace = actualSolutionModel.Solutions[0].Projects[0].Namespaces[0];
            Assert.Equal("object", modelsNamespace.ClassModels[0].BaseClassFullName);
            Assert.Equal("Models.Class1", modelsNamespace.ClassModels[1].BaseClassFullName);
            Assert.Equal("Models.Class1", modelsNamespace.ClassModels[2].BaseClassFullName);
            Assert.Equal("Models.Class3", modelsNamespace.ClassModels[3].BaseClassFullName);

            var otherModelsNamespace = actualSolutionModel.Solutions[0].Projects[0].Namespaces[1];
            Assert.Equal("Models.Other.Class2", otherModelsNamespace.ClassModels[0].BaseClassFullName);
            Assert.Equal("Models.Class1", otherModelsNamespace.ClassModels[1].BaseClassFullName);
            Assert.Equal("Models.TheClass", otherModelsNamespace.ClassModels[2].BaseClassFullName);
            Assert.Equal("Models.Other.Class3", otherModelsNamespace.ClassModels[3].BaseClassFullName);

            Assert.Equal("Models.Other.SuperClass",
                actualSolutionModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0]
                    .BaseClassFullName);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullBaseInterfacesNames_WhenGivenClassModels()
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
                        FullName = "IInterface1"
                    },
                    new ClassModel
                    {
                        FullName = "Models.Interfaces.IInterface2",
                        BaseInterfaces = {"IInterface1"}
                    },
                    new ClassModel
                    {
                        FullName = "Models.Interfaces.IInterface3",
                        BaseInterfaces = {"Models.Interfaces.IInterface1", "IInterface2"}
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
                        FullName = "Models.Class1",
                        BaseInterfaces = {"IInterface3"}
                    },
                    new ClassModel
                    {
                        FullName = "Class2",
                        BaseInterfaces = {"IInterface1"}
                    },
                    new ClassModel
                    {
                        FullName = "Models.Class3",
                        BaseInterfaces = {"IInterface1", "IInterface2", "Models.Interfaces.IInterface3"}
                    },
                    new ClassModel
                    {
                        FullName = "Models.TheClass",
                        BaseInterfaces = {"Models.Interfaces.IInterface1", "AInterface"}
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
                        FullName = "AInterface",
                    }
                }
            });

            solutionModel.Projects.Add(projectModel2);

            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            var modelInterfacesNamespace = actualSolutionModel.Solutions[0].Projects[0].Namespaces[0];
            Assert.Empty(modelInterfacesNamespace.ClassModels[0].BaseInterfaces);
            Assert.Equal("Models.Interfaces.IInterface1", modelInterfacesNamespace.ClassModels[1].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface1", modelInterfacesNamespace.ClassModels[2].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface2", modelInterfacesNamespace.ClassModels[2].BaseInterfaces[1]);

            var modelsNamespace = actualSolutionModel.Solutions[0].Projects[0].Namespaces[1];
            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassModels[0].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[1].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[2].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface2", modelsNamespace.ClassModels[2].BaseInterfaces[1]);
            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassModels[2].BaseInterfaces[2]);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[3].BaseInterfaces[0]);
            Assert.Equal("MyNamespace.AInterface", modelsNamespace.ClassModels[3].BaseInterfaces[1]);

            Assert.Empty(actualSolutionModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0]
                .BaseInterfaces);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullNamesOfContainingClassNameOfMethods_WhenGivenClassModelsWithMethods()
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
                        FullName = "Class1",
                        Methods =
                        {
                            new MethodModel
                            {
                                ContainingClassName = "Class1"
                            },
                            new MethodModel
                            {
                                ContainingClassName = "Project1.Models.Classes.Class1"
                            }
                        }
                    },
                    new ClassModel
                    {
                        FullName = "Project1.Models.Classes.Class2",
                        Methods =
                        {
                            new MethodModel
                            {
                                ContainingClassName = "Class2",
                            }
                        },
                        Constructors =
                        {
                            new MethodModel
                            {
                                ContainingClassName = "Class2"
                            },
                            new MethodModel
                            {
                                ContainingClassName = "Project1.Models.Classes.Class2"
                            }
                        }
                    },
                    new ClassModel
                    {
                        FullName = "Project1.Models.Classes.Class3",
                        Constructors =
                        {
                            new MethodModel
                            {
                                ContainingClassName = "Class3",
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel1);
            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            var modelInterfacesNamespace =
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[0];

            Assert.Empty(modelInterfacesNamespace.ClassModels[0].Constructors);
            Assert.Equal(2, modelInterfacesNamespace.ClassModels[0].Methods.Count);
            Assert.Equal("Project1.Models.Classes.Class1",
                modelInterfacesNamespace.ClassModels[0].Methods[0].ContainingClassName);
            Assert.Equal("Project1.Models.Classes.Class1",
                modelInterfacesNamespace.ClassModels[0].Methods[1].ContainingClassName);

            Assert.Equal(1, modelInterfacesNamespace.ClassModels[1].Methods.Count);
            Assert.Equal("Project1.Models.Classes.Class2",
                modelInterfacesNamespace.ClassModels[1].Methods[0].ContainingClassName);
            Assert.Equal(2, modelInterfacesNamespace.ClassModels[1].Constructors.Count);
            Assert.Equal("Project1.Models.Classes.Class2",
                modelInterfacesNamespace.ClassModels[1].Constructors[0].ContainingClassName);
            Assert.Equal("Project1.Models.Classes.Class2",
                modelInterfacesNamespace.ClassModels[1].Constructors[1].ContainingClassName);

            Assert.Empty(modelInterfacesNamespace.ClassModels[2].Methods);
            Assert.Equal(1, modelInterfacesNamespace.ClassModels[2].Constructors.Count);
            Assert.Equal("Project1.Models.Classes.Class3",
                modelInterfacesNamespace.ClassModels[2].Constructors[0].ContainingClassName);
        }

        [Fact]
        public void
            GetFunction_ShouldHaveFindClassInOtherProject_WhenGivenAClassThatCouldNotBeenFoundInCurrentProject()
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
                        FullName = "SomeModel"
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
                        FullName = "Service",
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


            solutionModel.Projects.Add(projectModel2);
            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("Models.SomeModel",
                actualSolutionModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].Fields[0].Type);
        }

        [Fact]
        public void
            GetFunction_ShouldHaveFindClassInOtherSolution_WhenGivenAClassThatCouldNotBeenFoundInCurrentSolution()
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
                        FullName = "SomeModel"
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
                        FullName = "Service",
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
                        FullName = "Controller",
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
                                ReturnType = "Service"
                            }
                        }
                    }
                }
            });

            solutionModel2.Projects.Add(projectModel3);

            repositoryModel.Solutions.Add(solutionModel1);
            repositoryModel.Solutions.Add(solutionModel2);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("Models.SomeModel",
                actualSolutionModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0]
                    .Fields[0].Type);
            Assert.Equal("Services.Service",
                actualSolutionModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0]
                    .Methods[0].ReturnType);
        }

        [Fact]
        public void
            GetFunction_ShouldHavePropertiesWithAmbiguousTypesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameProject()
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
                        FullName = "AmbiguousClass"
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
                        FullName = "AmbiguousClass"
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
                        FullName = "AmbiguousClass"
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
                        FullName = "SomeClass",
                        BaseClassFullName = "AmbiguousClass",
                        BaseInterfaces =
                        {
                            "AmbiguousClass"
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
                            new MethodModel
                            {
                                ParameterTypes =
                                {
                                    new ParameterModel
                                    {
                                        Type = "AmbiguousClass"
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
                                        Type = "AmbiguousClass"
                                    }
                                },
                                CalledMethods =
                                {
                                    new MethodCallModel
                                    {
                                        ContainingClassName = "AmbiguousClass",
                                        ParameterTypes =
                                        {
                                            new ParameterModel
                                            {
                                                Type = "AmbiguousClass"
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        Metrics =
                        {
                            new ClassMetric
                            {
                                ExtractorName = typeof(ParameterDependencyMetric).FullName,
                                ValueType = typeof(DependencyDataMetric).FullName,
                                Value = new DependencyDataMetric
                                {
                                    Dependencies = new Dictionary<string, int>()
                                    {
                                        {"AmbiguousClass", 2}
                                    },
                                }
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel);

            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("Models.AmbiguousClass",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].FullName);
            Assert.Equal("Services.AmbiguousClass",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0].FullName);
            Assert.Equal("Controllers.AmbiguousClass",
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].FullName);

            var someClassModel =
                actualSolutionModel.Solutions[0].Projects[0].Namespaces[3].ClassModels[0];
            Assert.Equal("SomeNamespace.SomeClass",
                someClassModel.FullName);

            Assert.Equal("AmbiguousClass", someClassModel.BaseClassFullName);
            Assert.Equal("AmbiguousClass", someClassModel.BaseInterfaces[0]);
            Assert.Equal("AmbiguousClass", someClassModel.Constructors[0].ParameterTypes[0].Type);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].ParameterTypes[0].Type);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].ContainingClassName);
            Assert.Equal("AmbiguousClass", someClassModel.Methods[0].CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("AmbiguousClass", someClassModel.Fields[0].Type);
            var metricDependencies = ((DependencyDataMetric) someClassModel.Metrics[0].Value).Dependencies;
            Assert.Equal(1, metricDependencies.Count);
            Assert.True(metricDependencies.ContainsKey("AmbiguousClass"));

            _progressLoggerMock.Verify(logger => logger.LogLine($"Multiple full names found for AmbiguousClass: "),
                Times.Once);
            _progressLoggerMock.Verify(logger => logger.LogLine($"Models.AmbiguousClass"));
            _progressLoggerMock.Verify(logger => logger.LogLine($"Services.AmbiguousClass"));
            _progressLoggerMock.Verify(logger => logger.LogLine($"Controllers.AmbiguousClass"));
        }

        [Fact]
        public void
            GetFunction_ShouldHavePropertiesWithAmbiguousNamesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameSolution()
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
                        FullName = "MyService"
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
                        FullName = "MyService"
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
                        FullName = "MyController",
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

            repositoryModel.Solutions.Add(solutionModel);

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("MyService",
                actualSolutionModel.Solutions[0].Projects[2].Namespaces[0].ClassModels[0].Fields[0].Type);

            _progressLoggerMock.Verify(logger => logger.LogLine($"Multiple full names found for MyService: "),
                Times.Once);
            _progressLoggerMock.Verify(logger => logger.LogLine($"Project1.Services.MyService"));
            _progressLoggerMock.Verify(logger => logger.LogLine($"Project2.Services.MyService"));
        }

        [Fact]
        public void
            GetFunction_ShouldHavePropertiesWithAmbiguousNamesAndLogThem_WhenGivenClassesWithTheSameNameInTheSameRepository()
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
                        FullName = "MyService"
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
                        FullName = "MyService"
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
                        FullName = "MyController",
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

            repositoryModel.Solutions.Add(solutionModel1);
            repositoryModel.Solutions.Add(solutionModel2);
            repositoryModel.Solutions.Add(solutionModel3);


            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("MyService",
                actualSolutionModel.Solutions[2].Projects[0].Namespaces[0].ClassModels[0].Fields[0].Type);

            _progressLoggerMock.Verify(logger => logger.LogLine($"Multiple full names found for MyService: "),
                Times.Once);
            _progressLoggerMock.Verify(logger => logger.LogLine($"Project1.Services.MyService"));
            _progressLoggerMock.Verify(logger => logger.LogLine($"Project2.Services.MyService"));
        }

        [Fact]
        public void
            GetFunction_ShouldHaveClassNameTheSame_WhenClassNameDoesNotExistInRepository()
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
                        FullName = "MyService"
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
                        FullName = "MyService"
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
                        FullName = "MyController",
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

            var processable = new ProcessorChain(IProcessable.Of(repositoryModel))
                .Process(_sut)
                .Finish<RepositoryModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("OutOfRepositoryClass",
                actualSolutionModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0].Fields[0].Type);

            _progressLoggerMock.Verify(logger => logger.LogLine($"Multiple full names found for MyService: "),
                Times.Never);
        }
    }
}
