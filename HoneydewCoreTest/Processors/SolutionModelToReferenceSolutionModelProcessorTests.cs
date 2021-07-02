using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations.ReferenceModel;
using HoneydewCore.Processors;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class SolutionModelToReferenceSolutionModelProcessorTests
    {
        private readonly SolutionModelToReferenceSolutionModelProcessor _sut;
        private readonly Mock<IMissingClassModelsHandler> _missingClassReferenceMock = new();

        public SolutionModelToReferenceSolutionModelProcessorTests()
        {
            _sut = new SolutionModelToReferenceSolutionModelProcessor(_missingClassReferenceMock.Object);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyProjects_WhenSolutionModelIsNull()
        {
            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(null));
            Assert.Empty(processable.Value.Projects);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyProjects_WhenSolutionModelIsEmpty()
        {
            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(new SolutionModel()));
            Assert.Empty(processable.Value.Projects);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithProjectsAndNamespaces_WhenGivenASolutionModelWithProjectAndNamespaces()
        {
            var solutionModel = new SolutionModel
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
                                    Name = "Project1.Services"
                                }
                            }
                        }
                    },
                    new ProjectModel
                    {
                        Name = "Project2",
                        Namespaces =
                        {
                            {
                                "Project2.Services", new NamespaceModel
                                {
                                    Name = "Project2.Services"
                                }
                            },
                            {
                                "Project2.Models", new NamespaceModel
                                {
                                    Name = "Project2.Models"
                                }
                            }
                        }
                    }
                }
            };

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            var referenceSolutionModel = processable.Value;

            Assert.Equal(2, referenceSolutionModel.Projects.Count);

            var projectModel1 = referenceSolutionModel.Projects[0];
            Assert.Equal("Project1", projectModel1.Name);
            Assert.Equal(referenceSolutionModel, projectModel1.SolutionReference);
            Assert.Equal(1, projectModel1.Namespaces.Count);
            Assert.Equal("Project1.Services", projectModel1.Namespaces[0].Name);
            Assert.Equal(projectModel1, projectModel1.Namespaces[0].ProjectReference);

            var projectModel2 = referenceSolutionModel.Projects[1];
            Assert.Equal("Project2", projectModel2.Name);
            Assert.Equal(referenceSolutionModel, projectModel2.SolutionReference);
            Assert.Equal(2, projectModel2.Namespaces.Count);
            Assert.Equal("Project2.Services", projectModel2.Namespaces[0].Name);
            Assert.Equal(projectModel2, projectModel2.Namespaces[0].ProjectReference);
            Assert.Equal("Project2.Models", projectModel2.Namespaces[1].Name);
            Assert.Equal(projectModel2, projectModel2.Namespaces[1].ProjectReference);
        }

        [Fact]
        public void GetFunction_ShouldReturnReferenceSolutionModelWithClasses_WhenGivenASolutionModelWithClasses()
        {
            var solutionModel = new SolutionModel
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
                                            FullName = "Project1.Services.CreateService",
                                            FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                        },
                                        new()
                                        {
                                            FullName = "Project1.Services.RetrieveService",
                                            FilePath = "validPathToProject/Project1/Services/RetrieveService.cs",
                                        }
                                    }
                                }
                            },
                            {
                                "Project1.Models", new NamespaceModel
                                {
                                    Name = "Project1.Models",
                                    ClassModels = new List<ClassModel>
                                    {
                                        new()
                                        {
                                            FullName = "Project1.Models.MyModel",
                                            FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };


            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            var referenceSolutionModel = processable.Value;

            Assert.Equal(1, referenceSolutionModel.Projects.Count);

            var projectModel1 = referenceSolutionModel.Projects[0];
            Assert.Equal("Project1", projectModel1.Name);
            Assert.Equal(referenceSolutionModel, projectModel1.SolutionReference);
            Assert.Equal(2, projectModel1.Namespaces.Count);


            var referenceNamespaceModel1 = projectModel1.Namespaces[0];
            Assert.Equal("Project1.Services", referenceNamespaceModel1.Name);
            Assert.Equal(projectModel1, referenceNamespaceModel1.ProjectReference);
            Assert.Equal(2, referenceNamespaceModel1.ClassModels.Count);

            var referenceClassModel1 = referenceNamespaceModel1.ClassModels[0];
            Assert.Equal("Project1.Services.CreateService", referenceClassModel1.Name);
            Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", referenceClassModel1.FilePath);
            Assert.Equal(referenceNamespaceModel1, referenceClassModel1.NamespaceReference);

            var referenceClassModel2 = referenceNamespaceModel1.ClassModels[1];
            Assert.Equal("Project1.Services.RetrieveService", referenceClassModel2.Name);
            Assert.Equal("validPathToProject/Project1/Services/RetrieveService.cs", referenceClassModel2.FilePath);
            Assert.Equal(referenceNamespaceModel1, referenceClassModel2.NamespaceReference);

            var referenceNamespaceModel2 = projectModel1.Namespaces[1];
            Assert.Equal("Project1.Models", referenceNamespaceModel2.Name);
            Assert.Equal(projectModel1, referenceNamespaceModel2.ProjectReference);
            Assert.Equal(1, referenceNamespaceModel2.ClassModels.Count);

            var referenceClassModel3 = referenceNamespaceModel2.ClassModels[0];
            Assert.Equal("Project1.Models.MyModel", referenceClassModel3.Name);
            Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassModel3.FilePath);
            Assert.Equal(referenceNamespaceModel2, referenceClassModel3.NamespaceReference);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesWithNonPrimitiveTypesAsParameters()
        {
            var solutionModel = new SolutionModel
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
                                            FullName = "Project1.Services.CreateService",
                                            FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                            Methods = new List<MethodModel>
                                            {
                                                new()
                                                {
                                                    Name = "Create",
                                                    Modifier = "",
                                                    AccessModifier = "public",
                                                    ReturnType = "Project1.Models.MyModel",
                                                    ContainingClassName = "Project1.Services.CreateService",
                                                },
                                                new()
                                                {
                                                    Name = "Convert",
                                                    Modifier = "",
                                                    AccessModifier = "public",
                                                    ParameterTypes = {"Project1.Models.MyModel"},
                                                    ReturnType = "Project1.Models.MyModel",
                                                    ContainingClassName = "Project1.Services.CreateService",
                                                    CalledMethods =
                                                    {
                                                        new MethodCallModel
                                                        {
                                                            ContainingClassName = "Project1.Services.CreateService",
                                                            MethodName = "Create"
                                                        }
                                                    }
                                                },
                                                new()
                                                {
                                                    Name = "Convert",
                                                    Modifier = "",
                                                    AccessModifier = "public",
                                                    ParameterTypes = {"Project1.Models.OtherModel"},
                                                    ReturnType = "Project1.Models.MyModel",
                                                    ContainingClassName = "Project1.Services.CreateService",
                                                    CalledMethods =
                                                    {
                                                        new MethodCallModel
                                                        {
                                                            ContainingClassName = "Project1.Services.CreateService",
                                                            MethodName = "Convert",
                                                            ParameterTypes = {"Project1.Models.MyModel"}
                                                        }
                                                    }
                                                },
                                                new()
                                                {
                                                    Name = "Process",
                                                    Modifier = "",
                                                    AccessModifier = "public",
                                                    ParameterTypes =
                                                        {"Project1.Models.MyModel", "Project1.Models.MyModel"},
                                                    ReturnType = "Project1.Models.MyModel",
                                                    ContainingClassName = "Project1.Services.CreateService",
                                                    CalledMethods =
                                                    {
                                                        new MethodCallModel
                                                        {
                                                            ContainingClassName = "Project1.Services.CreateService",
                                                            MethodName = "Create"
                                                        },
                                                        new MethodCallModel
                                                        {
                                                            ContainingClassName = "Project1.Services.CreateService",
                                                            MethodName = "Convert",
                                                            ParameterTypes = {"Project1.Models.OtherModel"}
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                    }
                                }
                            },
                            {
                                "Project1.Models", new NamespaceModel
                                {
                                    Name = "Project1.Models",
                                    ClassModels = new List<ClassModel>
                                    {
                                        new()
                                        {
                                            FullName = "Project1.Models.MyModel",
                                            FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                        },
                                        new()
                                        {
                                            FullName = "Project1.Models.OtherModel",
                                            FilePath = "validPathToProject/Project1/Models/OtherModel.cs",
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _missingClassReferenceMock.Setup(handler => handler.GetAllReferences())
                .Returns(new List<ReferenceClassModel>());

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            var referenceSolutionModel = processable.Value;

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            Assert.Empty(referenceSolutionModel.ClassModelsNotDeclaredInSolution);

            var projectModel1 = referenceSolutionModel.Projects[0];

            var referenceNamespaceServices = projectModel1.Namespaces[0];
            var referenceClassCreateService = referenceNamespaceServices.ClassModels[0];

            Assert.Equal("Project1", projectModel1.Name);
            Assert.Equal(referenceSolutionModel, projectModel1.SolutionReference);
            Assert.Equal(2, projectModel1.Namespaces.Count);

            Assert.Equal("Project1.Services", referenceNamespaceServices.Name);
            Assert.Equal(projectModel1, referenceNamespaceServices.ProjectReference);
            Assert.Equal(1, referenceNamespaceServices.ClassModels.Count);


            Assert.Equal("Project1.Services.CreateService", referenceClassCreateService.Name);
            Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", referenceClassCreateService.FilePath);
            Assert.Equal(referenceNamespaceServices, referenceClassCreateService.NamespaceReference);
            Assert.Empty(referenceClassCreateService.Fields);
            Assert.Equal(4, referenceClassCreateService.Methods.Count);

            var referenceCreateMethodModel = referenceClassCreateService.Methods[0];
            var referenceConvertMethodModel1 = referenceClassCreateService.Methods[1];
            var referenceConvertMethodModel2 = referenceClassCreateService.Methods[2];
            var referenceProcessMethodModel = referenceClassCreateService.Methods[3];

            var referenceNamespaceModels = projectModel1.Namespaces[1];
            var referenceClassMyModel = referenceNamespaceModels.ClassModels[0];
            var referenceClassOtherModel = referenceNamespaceModels.ClassModels[1];

            Assert.Equal("Create", referenceCreateMethodModel.Name);
            Assert.Equal(referenceClassCreateService, referenceCreateMethodModel.ContainingClass);
            Assert.Equal("", referenceCreateMethodModel.Modifier);
            Assert.Equal("public", referenceCreateMethodModel.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceCreateMethodModel.ReturnTypeReferenceClassModel);
            Assert.Empty(referenceCreateMethodModel.CalledMethods);
            Assert.Empty(referenceCreateMethodModel.ParameterTypes);

            Assert.Equal("Convert", referenceConvertMethodModel1.Name);
            Assert.Equal(referenceClassCreateService, referenceConvertMethodModel1.ContainingClass);
            Assert.Equal("", referenceConvertMethodModel1.Modifier);
            Assert.Equal("public", referenceConvertMethodModel1.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceConvertMethodModel1.ReturnTypeReferenceClassModel);
            Assert.Equal(1, referenceConvertMethodModel1.ParameterTypes.Count);
            Assert.Equal(referenceClassMyModel, referenceConvertMethodModel1.ParameterTypes[0]);
            Assert.Equal(1, referenceConvertMethodModel1.CalledMethods.Count);
            Assert.Equal(referenceCreateMethodModel, referenceConvertMethodModel1.CalledMethods[0]);

            Assert.Equal("Convert", referenceConvertMethodModel2.Name);
            Assert.Equal(referenceClassCreateService, referenceConvertMethodModel2.ContainingClass);
            Assert.Equal("", referenceConvertMethodModel2.Modifier);
            Assert.Equal("public", referenceConvertMethodModel2.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceConvertMethodModel2.ReturnTypeReferenceClassModel);
            Assert.Equal(1, referenceConvertMethodModel2.ParameterTypes.Count);
            Assert.Equal(referenceClassOtherModel, referenceConvertMethodModel2.ParameterTypes[0]);
            Assert.Equal(1, referenceConvertMethodModel2.CalledMethods.Count);
            Assert.Equal(referenceConvertMethodModel1, referenceConvertMethodModel2.CalledMethods[0]);

            Assert.Equal("Process", referenceProcessMethodModel.Name);
            Assert.Equal(referenceClassCreateService, referenceProcessMethodModel.ContainingClass);
            Assert.Equal("", referenceProcessMethodModel.Modifier);
            Assert.Equal("public", referenceProcessMethodModel.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.ReturnTypeReferenceClassModel);
            Assert.Equal(2, referenceProcessMethodModel.ParameterTypes.Count);
            Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.ParameterTypes[0]);
            Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.ParameterTypes[0]);
            Assert.Equal(2, referenceProcessMethodModel.CalledMethods.Count);
            Assert.Equal(referenceCreateMethodModel, referenceProcessMethodModel.CalledMethods[0]);
            Assert.Equal(referenceConvertMethodModel2, referenceProcessMethodModel.CalledMethods[1]);


            Assert.Equal("Project1.Models", referenceNamespaceModels.Name);
            Assert.Equal(projectModel1, referenceNamespaceModels.ProjectReference);
            Assert.Equal(2, referenceNamespaceModels.ClassModels.Count);

            Assert.Equal("Project1.Models.MyModel", referenceClassMyModel.Name);
            Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassMyModel.FilePath);
            Assert.Equal(referenceNamespaceModels, referenceClassMyModel.NamespaceReference);

            Assert.Equal("Project1.Models.OtherModel", referenceClassOtherModel.Name);
            Assert.Equal("validPathToProject/Project1/Models/OtherModel.cs", referenceClassOtherModel.FilePath);
            Assert.Equal(referenceNamespaceModels, referenceClassOtherModel.NamespaceReference);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithPrimitiveTypesAsParameters_UsingCSharpClassFactExtractor()
        {
            const string fileContent = @"
namespace Project1.Services
{
    public class MyClass
    {
        public float Function1(int a, int b)
        {
            var aString = Function3(a);
            var bString = Function3(b);

            var aInt = Function2(aString);
            var bInt = Function2(bString);

            var c = aInt + bInt;
            
            Print(c);
            
            return c;
        }
        
        public int Function2(string s)
        {
            return int.Parse(s);
        }

        public string Function3(int a)
        {
            return a.ToString();
        }

        private static void Print(float o)
        {
        }

        private void Print(int a)
        {
            if (a > 0)
            {
                Print(--a);
            }
        }
    }
}";

            var voidClassModel = new ReferenceClassModel
            {
                Name = "void"
            };

            var stringClassModel = new ReferenceClassModel
            {
                Name = "string",
            };

            var floatClassModel = new ReferenceClassModel
            {
                Name = "float",
            };

            var intToStringReferenceMethod = new ReferenceMethodModel()
            {
                Name = "ToString",
                ReturnTypeReferenceClassModel = stringClassModel
            };
            var intClassModel = new ReferenceClassModel
            {
                Name = "int",
                Methods =
                {
                    intToStringReferenceMethod
                }
            };
            var intParseMethodReference = new ReferenceMethodModel
            {
                Name = "Parse",
                ParameterTypes = {stringClassModel},
                ReturnTypeReferenceClassModel = intClassModel
            };
            intClassModel.Methods.Add(intParseMethodReference);

            _missingClassReferenceMock.Setup(handler => handler.GetAndAddReference("int")).Returns(intClassModel);
            _missingClassReferenceMock.Setup(handler => handler.GetAndAddReference("string")).Returns(stringClassModel);
            _missingClassReferenceMock.Setup(handler => handler.GetAndAddReference("float")).Returns(floatClassModel);
            _missingClassReferenceMock.Setup(handler => handler.GetAndAddReference("void")).Returns(voidClassModel);

            _missingClassReferenceMock.Setup(handler => handler.GetAllReferences())
                .Returns(new List<ReferenceClassModel>
                {
                    intClassModel,
                    stringClassModel,
                    floatClassModel,
                    voidClassModel
                });

            var extractor = new CSharpClassFactExtractor();
            var classModels = extractor.Extract(fileContent);

            var solutionModel = new SolutionModel
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
                                    ClassModels = classModels,
                                }
                            }
                        }
                    }
                }
            };


            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            var referenceSolutionModel = processable.Value;

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            Assert.Equal(4, referenceSolutionModel.ClassModelsNotDeclaredInSolution.Count);
            Assert.Equal(intClassModel, referenceSolutionModel.ClassModelsNotDeclaredInSolution[0]);
            Assert.Equal(stringClassModel, referenceSolutionModel.ClassModelsNotDeclaredInSolution[1]);
            Assert.Equal(floatClassModel, referenceSolutionModel.ClassModelsNotDeclaredInSolution[2]);
            Assert.Equal(voidClassModel, referenceSolutionModel.ClassModelsNotDeclaredInSolution[3]);

            var projectModel1 = referenceSolutionModel.Projects[0];

            var referenceNamespaceServices = projectModel1.Namespaces[0];
            var referenceMyClass = referenceNamespaceServices.ClassModels[0];

            Assert.Equal(1, referenceNamespaceServices.ClassModels.Count);

            Assert.Equal("Project1.Services.MyClass", referenceMyClass.Name);
            Assert.Equal(referenceNamespaceServices, referenceMyClass.NamespaceReference);
            Assert.Empty(referenceMyClass.Fields);
            Assert.Equal(5, referenceMyClass.Methods.Count);

            var methodFunction1 = referenceMyClass.Methods[0];
            var methodFunction2 = referenceMyClass.Methods[1];
            var methodFunction3 = referenceMyClass.Methods[2];
            var methodPrint1 = referenceMyClass.Methods[3];
            var methodPrint2 = referenceMyClass.Methods[4];

            Assert.Equal("Function1", methodFunction1.Name);
            Assert.Equal(referenceMyClass, methodFunction1.ContainingClass);
            Assert.Equal("", methodFunction1.Modifier);
            Assert.Equal("public", methodFunction1.AccessModifier);
            Assert.Equal(floatClassModel, methodFunction1.ReturnTypeReferenceClassModel);
            Assert.Equal(2, methodFunction1.ParameterTypes.Count);
            Assert.Equal(intClassModel, methodFunction1.ParameterTypes[0]);
            Assert.Equal(intClassModel, methodFunction1.ParameterTypes[1]);
            Assert.Equal(5, methodFunction1.CalledMethods.Count);
            Assert.Equal(methodFunction3, methodFunction1.CalledMethods[0]);
            Assert.Equal(methodFunction3, methodFunction1.CalledMethods[1]);
            Assert.Equal(methodFunction2, methodFunction1.CalledMethods[2]);
            Assert.Equal(methodFunction2, methodFunction1.CalledMethods[3]);
            Assert.Equal(methodPrint2, methodFunction1.CalledMethods[4]);

            Assert.Equal("Function2", methodFunction2.Name);
            Assert.Equal(referenceMyClass, methodFunction2.ContainingClass);
            Assert.Equal("", methodFunction2.Modifier);
            Assert.Equal("public", methodFunction2.AccessModifier);
            Assert.Equal(intClassModel, methodFunction2.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodFunction2.ParameterTypes.Count);
            Assert.Equal(stringClassModel, methodFunction2.ParameterTypes[0]);
            Assert.Equal(1, methodFunction2.CalledMethods.Count);
            Assert.Equal(intParseMethodReference, methodFunction2.CalledMethods[0]);

            Assert.Equal("Function3", methodFunction3.Name);
            Assert.Equal(referenceMyClass, methodFunction3.ContainingClass);
            Assert.Equal("", methodFunction3.Modifier);
            Assert.Equal("public", methodFunction3.AccessModifier);
            Assert.Equal(stringClassModel, methodFunction3.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodFunction3.ParameterTypes.Count);
            Assert.Equal(intClassModel, methodFunction3.ParameterTypes[0]);
            Assert.Equal(1, methodFunction3.CalledMethods.Count);
            Assert.Equal(intToStringReferenceMethod, methodFunction3.CalledMethods[0]);

            Assert.Equal("Print", methodPrint1.Name);
            Assert.Equal(referenceMyClass, methodPrint1.ContainingClass);
            Assert.Equal("static", methodPrint1.Modifier);
            Assert.Equal("private", methodPrint1.AccessModifier);
            Assert.Equal(voidClassModel, methodPrint1.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodPrint1.ParameterTypes.Count);
            Assert.Equal(floatClassModel, methodPrint1.ParameterTypes[0]);
            Assert.Empty(methodPrint1.CalledMethods);

            Assert.Equal("Print", methodPrint2.Name);
            Assert.Equal(referenceMyClass, methodPrint2.ContainingClass);
            Assert.Equal("", methodPrint2.Modifier);
            Assert.Equal("private", methodPrint2.AccessModifier);
            Assert.Equal(voidClassModel, methodPrint2.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodPrint2.ParameterTypes.Count);
            Assert.Equal(intClassModel, methodPrint2.ParameterTypes[0]);
            Assert.Equal(1, methodPrint2.CalledMethods.Count);
            Assert.Equal(methodPrint2, methodPrint2.CalledMethods[0]);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithFieldReferences_WhenGivenASolutionModelWithClassesWithFieldReferences()
        {
            var solutionModel = new SolutionModel
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
                                            FullName = "Project1.Services.CreateService",
                                            FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                            Fields = new List<FieldModel>
                                            {
                                                new()
                                                {
                                                    Name = "Model",
                                                    Type = "Project1.Models.MyModel",
                                                    AccessModifier = "private",
                                                    IsEvent = false
                                                }
                                            }
                                        },
                                    }
                                }
                            },
                            {
                                "Project1.Models", new NamespaceModel
                                {
                                    Name = "Project1.Models",
                                    ClassModels = new List<ClassModel>
                                    {
                                        new()
                                        {
                                            FullName = "Project1.Models.MyModel",
                                            FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                            Fields = new List<FieldModel>
                                            {
                                                new()
                                                {
                                                    Name = "_value",
                                                    Type = "int",
                                                    Modifier = "readonly",
                                                    AccessModifier = "private"
                                                },
                                                new()
                                                {
                                                    Name = "ValueEvent",
                                                    Type = "int",
                                                    Modifier = "",
                                                    AccessModifier = "public",
                                                    IsEvent = true
                                                }
                                            }
                                        },
                                        new()
                                        {
                                            FullName = "Project1.Models.OtherModel",
                                            FilePath = "validPathToProject/Project1/Models/OtherModel.cs",
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var intClassModel = new ReferenceClassModel
            {
                Name = "int"
            };

            _missingClassReferenceMock.Setup(handler => handler.GetAndAddReference("int"))
                .Returns(intClassModel);
            _missingClassReferenceMock.Setup(handler => handler.GetAllReferences())
                .Returns(new List<ReferenceClassModel> {intClassModel});

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            var referenceSolutionModel = processable.Value;

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            Assert.Equal(1,referenceSolutionModel.ClassModelsNotDeclaredInSolution.Count);
            Assert.Equal(intClassModel,referenceSolutionModel.ClassModelsNotDeclaredInSolution[0]);

            var projectModel1 = referenceSolutionModel.Projects[0];

            var referenceNamespaceServices = projectModel1.Namespaces[0];
            var referenceClassCreateService = referenceNamespaceServices.ClassModels[0];

            var referenceNamespaceModels = projectModel1.Namespaces[1];
            var referenceClassMyModel = referenceNamespaceModels.ClassModels[0];
            var referenceClassOtherModel = referenceNamespaceModels.ClassModels[1];


            Assert.Equal("Project1.Services.CreateService", referenceClassCreateService.Name);
            Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", referenceClassCreateService.FilePath);
            Assert.Equal(referenceNamespaceServices, referenceClassCreateService.NamespaceReference);
            Assert.Empty(referenceClassCreateService.Methods);
            Assert.Equal(1, referenceClassCreateService.Fields.Count);

            var createServiceClassField = referenceClassCreateService.Fields[0];
            Assert.Equal("Model", createServiceClassField.Name);
            Assert.Equal(referenceClassMyModel, createServiceClassField.Type);
            Assert.Equal(referenceClassCreateService, createServiceClassField.ContainingClass);
            Assert.Equal("", createServiceClassField.Modifier);
            Assert.Equal("private", createServiceClassField.AccessModifier);
            Assert.False(createServiceClassField.IsEvent);


            Assert.Equal("Project1.Models.MyModel", referenceClassMyModel.Name);
            Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassMyModel.FilePath);
            Assert.Equal(referenceNamespaceModels, referenceClassMyModel.NamespaceReference);
            Assert.Empty(referenceClassMyModel.Methods);
            Assert.Equal(2, referenceClassMyModel.Fields.Count);
            
            var valueFieldModel = referenceClassMyModel.Fields[0];
            Assert.Equal("_value", valueFieldModel.Name);
            Assert.Equal(intClassModel, valueFieldModel.Type);
            Assert.Equal(referenceClassMyModel, valueFieldModel.ContainingClass);
            Assert.Equal("readonly", valueFieldModel.Modifier);
            Assert.Equal("private", valueFieldModel.AccessModifier);
            Assert.False(valueFieldModel.IsEvent);
            
            var valueEventFieldModel = referenceClassMyModel.Fields[1];
            Assert.Equal("ValueEvent", valueEventFieldModel.Name);
            Assert.Equal(intClassModel, valueEventFieldModel.Type);
            Assert.Equal(referenceClassMyModel, valueEventFieldModel.ContainingClass);
            Assert.Equal("", valueEventFieldModel.Modifier);
            Assert.Equal("public", valueEventFieldModel.AccessModifier);
            Assert.True(valueEventFieldModel.IsEvent);

            Assert.Equal("Project1.Models.OtherModel", referenceClassOtherModel.Name);
            Assert.Equal("validPathToProject/Project1/Models/OtherModel.cs", referenceClassOtherModel.FilePath);
            Assert.Equal(referenceNamespaceModels, referenceClassOtherModel.NamespaceReference);
            Assert.Empty(referenceClassOtherModel.Fields);
            Assert.Empty(referenceClassOtherModel.Methods);
        }
    }
}