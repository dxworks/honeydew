using HoneydewCore.Models;
using HoneydewCore.Processors;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class FullNameModelProcessorTests
    {
        private readonly FullNameModelProcessor _sut;

        public FullNameModelProcessorTests()
        {
            _sut = new FullNameModelProcessor();
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassNames_WhenGivenClassModels()
        {
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add("Models", new NamespaceModel
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

            projectModel1.Namespaces.Add("Services", new NamespaceModel
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

            projectModel1.Namespaces.Add("Controllers", new NamespaceModel
            {
                Name = "Models",
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

            projectModel2.Namespaces.Add("Domain.Data", new NamespaceModel
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

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var actualSolutionModel = processable.Value;

            Assert.Equal("Models.Class1", actualSolutionModel.Projects[0].Namespaces["Models"].ClassModels[0].FullName);
            Assert.Equal("Services.Class2",
                actualSolutionModel.Projects[0].Namespaces["Services"].ClassModels[0].FullName);
            Assert.Equal("Controllers.Class3",
                actualSolutionModel.Projects[0].Namespaces["Controllers"].ClassModels[0].FullName);
            Assert.Equal("Controllers.Class4",
                actualSolutionModel.Projects[0].Namespaces["Controllers"].ClassModels[1].FullName);
            Assert.Equal("Domain.Data.Class5",
                actualSolutionModel.Projects[1].Namespaces["Domain.Data"].ClassModels[0].FullName);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassName_WhenGivenClassWithInnerClass()
        {
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add("Project1.Models", new NamespaceModel
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

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var actualSolutionModel = processable.Value;

            var namespaceModel = actualSolutionModel.Projects[0].Namespaces["Project1.Models"];
            Assert.Equal("Project1.Models.Class1", namespaceModel.ClassModels[0].FullName);
            Assert.Equal("Project1.Models.Class1.InnerClass1", namespaceModel.ClassModels[1].FullName);
            Assert.Equal("Project1.Models.Class1.InnerClass1.InnerClass2", namespaceModel.ClassModels[2].FullName);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullBaseClassNames_WhenGivenClassModels()
        {
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();

            projectModel1.Namespaces.Add("Models", new NamespaceModel
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

            projectModel1.Namespaces.Add("Models.Other", new NamespaceModel
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

            projectModel2.Namespaces.Add("MyNamespace", new NamespaceModel
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

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var actualSolutionModel = processable.Value;

            var modelsNamespace = actualSolutionModel.Projects[0].Namespaces["Models"];
            Assert.Equal("object", modelsNamespace.ClassModels[0].BaseClassFullName);
            Assert.Equal("Models.Class1", modelsNamespace.ClassModels[1].BaseClassFullName);
            Assert.Equal("Models.Class1", modelsNamespace.ClassModels[2].BaseClassFullName);
            Assert.Equal("Models.Class3", modelsNamespace.ClassModels[3].BaseClassFullName);

            var otherModelsNamespace = actualSolutionModel.Projects[0].Namespaces["Models.Other"];
            Assert.Equal("Models.Other.Class2", otherModelsNamespace.ClassModels[0].BaseClassFullName);
            Assert.Equal("Models.Class1", otherModelsNamespace.ClassModels[1].BaseClassFullName);
            Assert.Equal("Models.TheClass", otherModelsNamespace.ClassModels[2].BaseClassFullName);
            Assert.Equal("Models.Other.Class3", otherModelsNamespace.ClassModels[3].BaseClassFullName);

            Assert.Equal("Models.Other.SuperClass",
                actualSolutionModel.Projects[1].Namespaces["MyNamespace"].ClassModels[0].BaseClassFullName);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullBaseInterfacesNames_WhenGivenClassModels()
        {
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();


            projectModel1.Namespaces.Add("Models.Interfaces", new NamespaceModel
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

            projectModel1.Namespaces.Add("Models", new NamespaceModel
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

            projectModel2.Namespaces.Add("MyNamespace", new NamespaceModel
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

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var actualSolutionModel = processable.Value;

            var modelInterfacesNamespace = actualSolutionModel.Projects[0].Namespaces["Models.Interfaces"];
            Assert.Empty(modelInterfacesNamespace.ClassModels[0].BaseInterfaces);
            Assert.Equal("Models.Interfaces.IInterface1", modelInterfacesNamespace.ClassModels[1].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface1", modelInterfacesNamespace.ClassModels[2].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface2", modelInterfacesNamespace.ClassModels[2].BaseInterfaces[1]);

            var modelsNamespace = actualSolutionModel.Projects[0].Namespaces["Models"];
            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassModels[0].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[1].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[2].BaseInterfaces[0]);
            Assert.Equal("Models.Interfaces.IInterface2", modelsNamespace.ClassModels[2].BaseInterfaces[1]);
            Assert.Equal("Models.Interfaces.IInterface3", modelsNamespace.ClassModels[2].BaseInterfaces[2]);
            Assert.Equal("Models.Interfaces.IInterface1", modelsNamespace.ClassModels[3].BaseInterfaces[0]);
            Assert.Equal("MyNamespace.AInterface", modelsNamespace.ClassModels[3].BaseInterfaces[1]);

            Assert.Empty(actualSolutionModel.Projects[1].Namespaces["MyNamespace"].ClassModels[0].BaseInterfaces);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullNamesOfContainingClassNameOfMethods_WhenGivenClassModelsWithMethods()
        {
            var solutionModel = new SolutionModel();

            var projectModel1 = new ProjectModel();


            projectModel1.Namespaces.Add("Project1.Models.Classes", new NamespaceModel
            {
                Name = "Project1.Models.Interfaces",
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

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var actualSolutionModel = processable.Value;

            var modelInterfacesNamespace = actualSolutionModel.Projects[0].Namespaces["Project1.Models.Classes"];

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
    }
}