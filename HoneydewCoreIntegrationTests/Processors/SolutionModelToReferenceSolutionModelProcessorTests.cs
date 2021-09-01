using System.Collections.Generic;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class SolutionModelToReferenceSolutionModelProcessorTests
    {
        private readonly SolutionModelToReferenceSolutionModelProcessor _sut;

        public SolutionModelToReferenceSolutionModelProcessorTests()
        {
            _sut = new SolutionModelToReferenceSolutionModelProcessor();
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyProjects_WhenSolutionModelIsNull()
        {
            var referenceSolutionModel = _sut.Process(null);
            Assert.Empty(referenceSolutionModel.Projects);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyProjects_WhenSolutionModelIsEmpty()
        {
            var referenceSolutionModel = _sut.Process(new SolutionModel());
            Assert.Empty(referenceSolutionModel.Projects);
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
                            new NamespaceModel
                            {
                                Name = "Project1.Services"
                            }
                        }
                    },
                    new ProjectModel
                    {
                        Name = "Project2",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                Name = "Project2.Services"
                            },
                            new NamespaceModel
                            {
                                Name = "Project2.Models"
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(solutionModel);

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
                            new NamespaceModel
                            {
                                Name = "Project1.Services",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "Project1.Services.CreateService",
                                        FilePath = "validPathToProject/Project1/Services/CreateService.cs"
                                    },
                                    new ClassModel()
                                    {
                                        Name = "Project1.Services.RetrieveService",
                                        FilePath = "validPathToProject/Project1/Services/RetrieveService.cs",
                                        Metrics = new List<MetricModel>
                                        {
                                            new()
                                            {
                                                Value = "0",
                                                ExtractorName = "SomeExtractor",
                                                ValueType = "int"
                                            }
                                        }
                                    }
                                }
                            },
                            new NamespaceModel
                            {
                                Name = "Project1.Models",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "Project1.Models.MyModel",
                                        FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                    }
                                }
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(solutionModel);

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
            Assert.Empty(referenceClassModel1.Metrics);

            var referenceClassModel2 = referenceNamespaceModel1.ClassModels[1];
            Assert.Equal("Project1.Services.RetrieveService", referenceClassModel2.Name);
            Assert.Equal("validPathToProject/Project1/Services/RetrieveService.cs", referenceClassModel2.FilePath);
            Assert.Equal(referenceNamespaceModel1, referenceClassModel2.NamespaceReference);
            Assert.Equal(1, referenceClassModel2.Metrics.Count);
            Assert.Equal("0", referenceClassModel2.Metrics[0].Value);
            Assert.Equal("int", referenceClassModel2.Metrics[0].ValueType);
            Assert.Equal("SomeExtractor", referenceClassModel2.Metrics[0].ExtractorName);

            var referenceNamespaceModel2 = projectModel1.Namespaces[1];
            Assert.Equal("Project1.Models", referenceNamespaceModel2.Name);
            Assert.Equal(projectModel1, referenceNamespaceModel2.ProjectReference);
            Assert.Equal(1, referenceNamespaceModel2.ClassModels.Count);

            var referenceClassModel3 = referenceNamespaceModel2.ClassModels[0];
            Assert.Equal("Project1.Models.MyModel", referenceClassModel3.Name);
            Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassModel3.FilePath);
            Assert.Equal(referenceNamespaceModel2, referenceClassModel3.NamespaceReference);
            Assert.Empty(referenceClassModel3.Metrics);
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
                            new NamespaceModel
                            {
                                Name = "Project1.Services",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "Project1.Services.CreateService",
                                        FilePath = "validPathToProject/Project1/Services/CreateService.cs",
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
                                        },
                                        Methods = new List<IMethodType>
                                        {
                                            new MethodModel
                                            {
                                                Name = "Create",
                                                Modifier = "",
                                                AccessModifier = "public",
                                                ReturnValue = new ReturnValueModel
                                                {
                                                    Type = new EntityTypeModel
                                                    {
                                                        Name = "Project1.Models.MyModel"
                                                    }
                                                },
                                                ContainingTypeName = "Project1.Services.CreateService",
                                            },
                                            new MethodModel
                                            {
                                                Name = "Convert",
                                                Modifier = "",
                                                AccessModifier = "public",
                                                ParameterTypes =
                                                {
                                                    new ParameterModel
                                                    {
                                                        Type = new EntityTypeModel
                                                        {
                                                            Name = "Project1.Models.MyModel"
                                                        }
                                                    }
                                                },
                                                ReturnValue = new ReturnValueModel
                                                {
                                                    Type = new EntityTypeModel
                                                    {
                                                        Name = "Project1.Models.MyModel"
                                                    }
                                                },
                                                ContainingTypeName = "Project1.Services.CreateService",
                                                CalledMethods =
                                                {
                                                    new MethodCallModel
                                                    {
                                                        ContainingTypeName = "Project1.Services.CreateService",
                                                        Name = "Create"
                                                    }
                                                }
                                            },
                                            new MethodModel
                                            {
                                                Name = "Convert",
                                                Modifier = "",
                                                AccessModifier = "public",
                                                ParameterTypes =
                                                {
                                                    new ParameterModel
                                                    {
                                                        Type = new EntityTypeModel
                                                        {
                                                            Name = "Project1.Models.OtherModel"
                                                        }
                                                    }
                                                },
                                                ReturnValue = new ReturnValueModel
                                                {
                                                    Type = new EntityTypeModel
                                                    {
                                                        Name = "Project1.Models.MyModel"
                                                    }
                                                },
                                                ContainingTypeName = "Project1.Services.CreateService",
                                                CalledMethods =
                                                {
                                                    new MethodCallModel
                                                    {
                                                        ContainingTypeName = "Project1.Services.CreateService",
                                                        Name = "Convert",
                                                        ParameterTypes =
                                                        {
                                                            new ParameterModel
                                                            {
                                                                Type = new EntityTypeModel
                                                                {
                                                                    Name = "Project1.Models.MyModel"
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new MethodModel
                                            {
                                                Name = "Process",
                                                Modifier = "",
                                                AccessModifier = "public",
                                                ParameterTypes =
                                                {
                                                    new ParameterModel
                                                    {
                                                        Type = new EntityTypeModel
                                                        {
                                                            Name = "Project1.Models.MyModel",
                                                        }
                                                    },
                                                    new ParameterModel
                                                    {
                                                        Type = new EntityTypeModel
                                                        {
                                                            Name = "Project1.Models.MyModel"
                                                        }
                                                    }
                                                },
                                                ReturnValue = new ReturnValueModel
                                                {
                                                    Type = new EntityTypeModel
                                                    {
                                                        Name = "Project1.Models.MyModel"
                                                    }
                                                },
                                                ContainingTypeName = "Project1.Services.CreateService",
                                                CalledMethods =
                                                {
                                                    new MethodCallModel
                                                    {
                                                        ContainingTypeName = "Project1.Services.CreateService",
                                                        Name = "Create"
                                                    },
                                                    new MethodCallModel
                                                    {
                                                        ContainingTypeName = "Project1.Services.CreateService",
                                                        Name = "Convert",
                                                        ParameterTypes =
                                                        {
                                                            new ParameterModel
                                                            {
                                                                Type = new EntityTypeModel
                                                                {
                                                                    Name = "Project1.Models.OtherModel"
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                }
                            },
                            new NamespaceModel
                            {
                                Name = "Project1.Models",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "Project1.Models.MyModel",
                                        FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                    },
                                    new ClassModel()
                                    {
                                        Name = "Project1.Models.OtherModel",
                                        FilePath = "validPathToProject/Project1/Models/OtherModel.cs",
                                    }
                                }
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);

            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
            Assert.Equal(1, allCreatedReferences.Count);
            var objectClassModel = allCreatedReferences[0];
            Assert.Equal("object", objectClassModel.Name);

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
            Assert.Empty(referenceCreateMethodModel.Parameters);

            Assert.Equal("Convert", referenceConvertMethodModel1.Name);
            Assert.Equal(referenceClassCreateService, referenceConvertMethodModel1.ContainingClass);
            Assert.Equal("", referenceConvertMethodModel1.Modifier);
            Assert.Equal("public", referenceConvertMethodModel1.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceConvertMethodModel1.ReturnTypeReferenceClassModel);
            Assert.Equal(1, referenceConvertMethodModel1.Parameters.Count);
            Assert.Equal(referenceClassMyModel, referenceConvertMethodModel1.Parameters[0].Type);
            Assert.Equal("", referenceConvertMethodModel1.Parameters[0].Modifier);
            Assert.Null(referenceConvertMethodModel1.Parameters[0].DefaultValue);
            Assert.Equal(1, referenceConvertMethodModel1.CalledMethods.Count);
            Assert.Equal(referenceCreateMethodModel, referenceConvertMethodModel1.CalledMethods[0]);

            Assert.Equal("Convert", referenceConvertMethodModel2.Name);
            Assert.Equal(referenceClassCreateService, referenceConvertMethodModel2.ContainingClass);
            Assert.Equal("", referenceConvertMethodModel2.Modifier);
            Assert.Equal("public", referenceConvertMethodModel2.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceConvertMethodModel2.ReturnTypeReferenceClassModel);
            Assert.Equal(1, referenceConvertMethodModel2.Parameters.Count);
            Assert.Equal(referenceClassOtherModel, referenceConvertMethodModel2.Parameters[0].Type);
            Assert.Equal("", referenceConvertMethodModel2.Parameters[0].Modifier);
            Assert.Null(referenceConvertMethodModel2.Parameters[0].DefaultValue);
            Assert.Equal(1, referenceConvertMethodModel2.CalledMethods.Count);
            Assert.Equal(referenceConvertMethodModel1, referenceConvertMethodModel2.CalledMethods[0]);

            Assert.Equal("Process", referenceProcessMethodModel.Name);
            Assert.Equal(referenceClassCreateService, referenceProcessMethodModel.ContainingClass);
            Assert.Equal("", referenceProcessMethodModel.Modifier);
            Assert.Equal("public", referenceProcessMethodModel.AccessModifier);
            Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.ReturnTypeReferenceClassModel);
            Assert.Equal(2, referenceProcessMethodModel.Parameters.Count);
            Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.Parameters[0].Type);
            Assert.Equal("", referenceProcessMethodModel.Parameters[0].Modifier);
            Assert.Null(referenceProcessMethodModel.Parameters[0].DefaultValue);
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
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
                    {
                        new MethodCallInfoVisitor()
                    }),
                    new ParameterSetterVisitor(new List<IParameterVisitor>
                    {
                        new ParameterInfoVisitor()
                    })
                })
            }));

            var extractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);

            var classModels = extractor.Extract(fileContent).ClassTypes;

            var solutionModel = new SolutionModel
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
                                ClassModels = classModels,
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
            Assert.Equal(5, allCreatedReferences.Count);

            var objectClassModel = allCreatedReferences[0];
            var intClassModel = allCreatedReferences[1];
            var floatClassModel = allCreatedReferences[2];
            var stringClassModel = allCreatedReferences[3];
            var voidClassModel = allCreatedReferences[4];

            Assert.Equal("int", intClassModel.Name);
            Assert.Equal("object", objectClassModel.Name);
            Assert.Equal("string", stringClassModel.Name);
            Assert.Equal("float", floatClassModel.Name);
            Assert.Equal("void", voidClassModel.Name);

            var intParseMethodReference = intClassModel.Methods[0];
            var intToStringReferenceMethod = intClassModel.Methods[1];

            Assert.Equal("ToString", intToStringReferenceMethod.Name);
            Assert.Empty(intToStringReferenceMethod.Parameters);

            Assert.Equal("Parse", intParseMethodReference.Name);
            Assert.Equal(1, intParseMethodReference.Parameters.Count);
            Assert.Equal(stringClassModel, intParseMethodReference.Parameters[0].Type);
            Assert.Equal("", intParseMethodReference.Parameters[0].Modifier);
            Assert.Null(intParseMethodReference.Parameters[0].DefaultValue);

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
            Assert.Equal(2, methodFunction1.Parameters.Count);
            Assert.Equal(intClassModel, methodFunction1.Parameters[0].Type);
            Assert.Equal("", methodFunction1.Parameters[0].Modifier);
            Assert.Null(methodFunction1.Parameters[0].DefaultValue);
            Assert.Equal(intClassModel, methodFunction1.Parameters[1].Type);
            Assert.Equal("", methodFunction1.Parameters[1].Modifier);
            Assert.Null(methodFunction1.Parameters[1].DefaultValue);
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
            Assert.Equal(1, methodFunction2.Parameters.Count);
            Assert.Equal(stringClassModel, methodFunction2.Parameters[0].Type);
            Assert.Equal("", methodFunction2.Parameters[0].Modifier);
            Assert.Null(methodFunction2.Parameters[0].DefaultValue);
            Assert.Equal(1, methodFunction2.CalledMethods.Count);
            Assert.Equal(intParseMethodReference, methodFunction2.CalledMethods[0]);

            Assert.Equal("Function3", methodFunction3.Name);
            Assert.Equal(referenceMyClass, methodFunction3.ContainingClass);
            Assert.Equal("", methodFunction3.Modifier);
            Assert.Equal("public", methodFunction3.AccessModifier);
            Assert.Equal(stringClassModel, methodFunction3.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodFunction3.Parameters.Count);
            Assert.Equal(intClassModel, methodFunction3.Parameters[0].Type);
            Assert.Equal("", methodFunction3.Parameters[0].Modifier);
            Assert.Null(methodFunction3.Parameters[0].DefaultValue);
            Assert.Equal(1, methodFunction3.CalledMethods.Count);
            Assert.Equal(intToStringReferenceMethod, methodFunction3.CalledMethods[0]);

            Assert.Equal("Print", methodPrint1.Name);
            Assert.Equal(referenceMyClass, methodPrint1.ContainingClass);
            Assert.Equal("static", methodPrint1.Modifier);
            Assert.Equal("private", methodPrint1.AccessModifier);
            Assert.Equal(voidClassModel, methodPrint1.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodPrint1.Parameters.Count);
            Assert.Equal(floatClassModel, methodPrint1.Parameters[0].Type);
            Assert.Equal("", methodPrint1.Parameters[0].Modifier);
            Assert.Null(methodPrint1.Parameters[0].DefaultValue);
            Assert.Empty(methodPrint1.CalledMethods);

            Assert.Equal("Print", methodPrint2.Name);
            Assert.Equal(referenceMyClass, methodPrint2.ContainingClass);
            Assert.Equal("", methodPrint2.Modifier);
            Assert.Equal("private", methodPrint2.AccessModifier);
            Assert.Equal(voidClassModel, methodPrint2.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodPrint2.Parameters.Count);
            Assert.Equal(intClassModel, methodPrint2.Parameters[0].Type);
            Assert.Equal("", methodPrint2.Parameters[0].Modifier);
            Assert.Null(methodPrint2.Parameters[0].DefaultValue);
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
                            new NamespaceModel
                            {
                                Name = "Project1.Services",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "Project1.Services.CreateService",
                                        FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                        Fields = new List<IFieldType>
                                        {
                                            new FieldModel
                                            {
                                                Name = "Model",
                                                Type = new EntityTypeModel
                                                {
                                                    Name = "Project1.Models.MyModel",
                                                },
                                                AccessModifier = "private",
                                                IsEvent = false
                                            }
                                        }
                                    },
                                }
                            },
                            new NamespaceModel
                            {
                                Name = "Project1.Models",
                                ClassModels = new List<IClassType>
                                {
                                    new ClassModel()
                                    {
                                        Name = "Project1.Models.MyModel",
                                        FilePath = "validPathToProject/Project1/Models/MyModel.cs",
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
                                        },
                                        Fields = new List<IFieldType>
                                        {
                                            new FieldModel
                                            {
                                                Name = "_value",
                                                Type = new EntityTypeModel
                                                {
                                                    Name = "int",
                                                },
                                                Modifier = "readonly",
                                                AccessModifier = "private"
                                            },
                                            new FieldModel
                                            {
                                                Name = "ValueEvent",
                                                Type = new EntityTypeModel
                                                {
                                                    Name = "int",
                                                },
                                                Modifier = "",
                                                AccessModifier = "public",
                                                IsEvent = true
                                            }
                                        }
                                    },
                                    new ClassModel()
                                    {
                                        Name = "Project1.Models.OtherModel",
                                        FilePath = "validPathToProject/Project1/Models/OtherModel.cs",
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
            Assert.Equal(2, allCreatedReferences.Count);

            var objectClassModel = allCreatedReferences[0];
            var intClassModel = allCreatedReferences[1];

            Assert.Equal("int", intClassModel.Name);
            Assert.Equal("object", objectClassModel.Name);

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
