﻿using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.MethodCalls;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
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
using HoneydewScriptBeePlugin.Loaders;
using HoneydewScriptBeePlugin.Models;
using Moq;
using Xunit;
using ClassModel = HoneydewModels.CSharp.ClassModel;
using FieldModel = HoneydewModels.CSharp.FieldModel;
using MethodModel = HoneydewModels.CSharp.MethodModel;
using NamespaceModel = HoneydewModels.CSharp.NamespaceModel;
using ParameterModel = HoneydewModels.CSharp.ParameterModel;
using ProjectModel = HoneydewModels.CSharp.ProjectModel;
using RepositoryModel = HoneydewModels.CSharp.RepositoryModel;
using ReturnValueModel = HoneydewModels.CSharp.ReturnValueModel;
using SolutionModel = HoneydewModels.CSharp.SolutionModel;

namespace HoneydewTests.Processors;

public class RepositoryModelToReferenceRepositoryModelProcessorTests
{
    private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public RepositoryModelToReferenceRepositoryModelProcessorTests()
    {
        _sut = new RepositoryModelToReferenceRepositoryModelProcessor();
    }

    [Fact]
    public void GetFunction_ShouldReturnEmptyProjects_WhenSolutionModelIsNull()
    {
        var referenceSolutionModel = _sut.Process(null);
        Assert.Empty(referenceSolutionModel.Solutions);
    }

    [Fact]
    public void GetFunction_ShouldReturnEmptyProjects_WhenSolutionModelIsEmpty()
    {
        var referenceSolutionModel = _sut.Process(new RepositoryModel());
        Assert.Empty(referenceSolutionModel.Solutions);
    }

    [Fact]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithProjectsAndNamespaces_WhenGivenASolutionModelWithProjectAndNamespaces()
    {
        var repositoryModel = new RepositoryModel
        {
            Solutions = new List<SolutionModel>
            {
                new()
                {
                    FilePath = "solutionPath",
                    ProjectsPaths = new List<string>
                    {
                        "project1.csproj",
                        "project2.csproj"
                    }
                }
            },
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    FilePath = "project1.csproj",
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
                    FilePath = "project2.csproj",
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

        var referenceRepositoryModel = _sut.Process(repositoryModel);

        Assert.Equal(1, referenceRepositoryModel.Solutions.Count);
        var referenceSolutionModel = referenceRepositoryModel.Solutions[0];

        Assert.Equal(2, referenceSolutionModel.Projects.Count);

        var projectModel1 = referenceSolutionModel.Projects[0];
        Assert.Equal("Project1", projectModel1.Name);
        Assert.Equal(referenceSolutionModel, projectModel1.Solutions[0]);
        Assert.Equal(1, projectModel1.Namespaces.Count);
        var projectModel1Namespace = projectModel1.Namespaces[0];
        Assert.Equal("Project1.Services", projectModel1Namespace.FullName);
        Assert.Equal("Services", projectModel1Namespace.Name);
        Assert.Equal(0, projectModel1Namespace.ChildNamespaces.Count);
        Assert.NotNull(projectModel1Namespace.Parent);
        Assert.Equal("Project1", projectModel1Namespace.Parent!.Name);
        Assert.Equal("Project1", projectModel1Namespace.Parent.FullName);

        var projectModel2 = referenceSolutionModel.Projects[1];
        Assert.Equal("Project2", projectModel2.Name);
        Assert.Equal(referenceSolutionModel, projectModel2.Solutions[0]);
        Assert.Equal(2, projectModel2.Namespaces.Count);
        var projectService2Namespace = projectModel2.Namespaces[0];
        Assert.Equal("Services", projectService2Namespace.Name);
        Assert.Equal("Project2.Services", projectService2Namespace.FullName);

        var projectModel2Namespace = projectModel2.Namespaces[1];
        Assert.Equal("Models", projectModel2Namespace.Name);
        Assert.Equal("Project2.Models", projectModel2Namespace.FullName);

        var parentNamespaces = new[]
        {
            projectService2Namespace.Parent,
            projectModel2Namespace.Parent,
        };

        foreach (var project2Namespace in parentNamespaces)
        {
            Assert.Equal("Project2", project2Namespace!.Name);
            Assert.Equal(2, project2Namespace.ChildNamespaces.Count);
            Assert.Equal("Services", project2Namespace.ChildNamespaces[0].Name);
            Assert.Equal("Project2.Services", project2Namespace.ChildNamespaces[0].FullName);
        }
    }

    [Fact]
    public void GetFunction_ShouldReturnReferenceSolutionModelWithClasses_WhenGivenASolutionModelWithClasses()
    {
        var repositoryModel = new RepositoryModel
        {
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    CompilationUnits =
                    {
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Services",
                            ClassTypes = new List<IClassType>
                            {
                                new ClassModel
                                {
                                    Name = "Project1.Services.CreateService",
                                    FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                    ContainingNamespaceName = "Project1.Services",
                                },
                                new ClassModel
                                {
                                    Name = "Project1.Services.RetrieveService",
                                    FilePath = "validPathToProject/Project1/Services/RetrieveService.cs",
                                    ContainingNamespaceName = "Project1.Services",
                                    Metrics = new List<MetricModel>
                                    {
                                        new()
                                        {
                                            Value = 15,
                                            ExtractorName = "SomeExtractor",
                                            ValueType = "int",
                                            Name = "MetricName",
                                        }
                                    }
                                }
                            }
                        },
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Models",
                            ClassTypes = new List<IClassType>
                            {
                                new ClassModel
                                {
                                    Name = "Project1.Models.MyModel",
                                    FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                    ContainingNamespaceName = "Project1.Models",
                                }
                            }
                        }
                    }
                }
            }
        };


        var referenceRepositoryModel = _sut.Process(repositoryModel);

        Assert.Equal(1, referenceRepositoryModel.Projects.Count);

        var projectModel1 = referenceRepositoryModel.Projects[0];
        Assert.Equal("Project1", projectModel1.Name);
        Assert.Equal(referenceRepositoryModel, projectModel1.Repository);
        Assert.Equal(2, projectModel1.Files.Count);


        var compilationUnitModel1 = projectModel1.Files[0];
        Assert.Equal("Project1.Services", compilationUnitModel1.FilePath);
        Assert.Equal(projectModel1, compilationUnitModel1.Project);
        Assert.Equal(2, compilationUnitModel1.Entities.Count);

        var referenceClassModel1 = compilationUnitModel1.Entities[0];
        Assert.Equal("Project1.Services.CreateService", referenceClassModel1.Name);
        Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", referenceClassModel1.FilePath);
        Assert.Equal(compilationUnitModel1, referenceClassModel1.File);
        Assert.Empty(referenceClassModel1.Metrics);

        var referenceClassModel2 = compilationUnitModel1.Entities[1];
        Assert.Equal("Project1.Services.RetrieveService", referenceClassModel2.Name);
        Assert.Equal("validPathToProject/Project1/Services/RetrieveService.cs", referenceClassModel2.FilePath);
        Assert.Equal(compilationUnitModel1, referenceClassModel2.File);
        Assert.Equal(1, referenceClassModel2.Metrics.Count);
        Assert.Equal(1, referenceClassModel2.Metrics.Count);
        Assert.Equal(14, referenceClassModel2.Metrics["MetricName"]);

        var referenceNamespaceModel2 = projectModel1.Files[1];
        Assert.Equal("Project1.Models", referenceNamespaceModel2.FilePath);
        Assert.Equal(projectModel1, referenceNamespaceModel2.Project);
        Assert.Equal(1, referenceNamespaceModel2.Entities.Count);

        var referenceClassModel3 = referenceNamespaceModel2.Entities[0];
        Assert.Equal("Project1.Models.MyModel", referenceClassModel3.Name);
        Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassModel3.FilePath);
        Assert.Equal(referenceNamespaceModel2, referenceClassModel3.File);
        Assert.Empty(referenceClassModel3.Metrics);
    }

    [Fact]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesWithNonPrimitiveTypesAsParameters()
    {
        var solutionModel = new RepositoryModel
        {
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    CompilationUnits =
                    {
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Services",
                            ClassTypes = new List<IClassType>
                            {
                                new ClassModel
                                {
                                    Name = "Project1.Services.CreateService",
                                    FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                    ContainingNamespaceName = "Project1.Services",
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
                                            CalledMethods =
                                            {
                                                new MethodCallModel
                                                {
                                                    LocationClassName = "Project1.Services.CreateService",
                                                    DefinitionClassName = "Project1.Services.CreateService",
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
                                            CalledMethods =
                                            {
                                                new MethodCallModel
                                                {
                                                    LocationClassName = "Project1.Services.CreateService",
                                                    DefinitionClassName = "Project1.Services.CreateService",
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
                                            CalledMethods =
                                            {
                                                new MethodCallModel
                                                {
                                                    LocationClassName = "Project1.Services.CreateService",
                                                    DefinitionClassName = "Project1.Services.CreateService",
                                                    Name = "Create"
                                                },
                                                new MethodCallModel
                                                {
                                                    LocationClassName = "Project1.Services.CreateService",
                                                    DefinitionClassName = "Project1.Services.CreateService",
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
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Models",
                            ClassTypes = new List<IClassType>
                            {
                                new ClassModel
                                {
                                    Name = "Project1.Models.MyModel",
                                    FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                    ContainingNamespaceName = "Project1.Models",
                                },
                                new ClassModel
                                {
                                    Name = "Project1.Models.OtherModel",
                                    FilePath = "validPathToProject/Project1/Models/OtherModel.cs",
                                    ContainingNamespaceName = "Project1.Models",
                                }
                            }
                        }
                    }
                }
            }
        };


        var referenceRepositoryModel = _sut.Process(solutionModel);

        Assert.Equal(1, referenceRepositoryModel.Projects.Count);

        var allCreatedReferences = referenceRepositoryModel.CreatedClasses;
        Assert.Equal(1, allCreatedReferences.Count);
        var objectClassModel = allCreatedReferences[0];
        Assert.Equal("object", objectClassModel.Name);

        var projectModel1 = referenceRepositoryModel.Projects[0];

        var compilationUnitModel = projectModel1.Files[0];
        var referenceClassCreateService = compilationUnitModel.Entities[0] as HoneydewScriptBeePlugin.Models.ClassModel;

        Assert.Equal("Project1", projectModel1.Name);
        Assert.Equal(referenceRepositoryModel, projectModel1.Repository);
        Assert.Equal(2, projectModel1.Files.Count);

        Assert.Equal("Project1.Services", compilationUnitModel.FilePath);
        Assert.Equal(projectModel1, compilationUnitModel.Project);
        Assert.Equal(1, compilationUnitModel.Entities.Count);


        Assert.NotNull(referenceClassCreateService);
        Assert.Equal("Project1.Services.CreateService", referenceClassCreateService!.Name);
        Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", referenceClassCreateService.FilePath);
        Assert.Equal(compilationUnitModel, referenceClassCreateService.File);
        Assert.Empty(referenceClassCreateService.Fields);
        Assert.Equal(4, referenceClassCreateService.Methods.Count);

        var referenceCreateMethodModel = referenceClassCreateService.Methods[0];
        var referenceConvertMethodModel1 = referenceClassCreateService.Methods[1];
        var referenceConvertMethodModel2 = referenceClassCreateService.Methods[2];
        var referenceProcessMethodModel = referenceClassCreateService.Methods[3];

        var referenceNamespaceModels = projectModel1.Files[1];
        var referenceClassMyModel = referenceNamespaceModels.Entities[0];
        var referenceClassOtherModel = referenceNamespaceModels.Entities[1];

        Assert.Equal("Create", referenceCreateMethodModel.Name);
        Assert.Equal(referenceClassCreateService, referenceCreateMethodModel.Entity);
        Assert.Equal("", referenceCreateMethodModel.Modifier);
        Assert.Equal(AccessModifier.Public, referenceCreateMethodModel.AccessModifier);
        Assert.NotNull(referenceCreateMethodModel.ReturnValue);
        Assert.Equal(referenceClassMyModel, referenceCreateMethodModel.ReturnValue!.Type.Entity);
        Assert.Empty(referenceCreateMethodModel.OutgoingCalls);
        Assert.Empty(referenceCreateMethodModel.Parameters);

        Assert.Equal("Convert", referenceConvertMethodModel1.Name);
        Assert.Equal(referenceClassCreateService, referenceConvertMethodModel1.Entity);
        Assert.Equal("", referenceConvertMethodModel1.Modifier);
        Assert.Equal(AccessModifier.Public, referenceConvertMethodModel1.AccessModifier);
        Assert.NotNull(referenceConvertMethodModel1.ReturnValue);
        Assert.Equal(referenceClassMyModel, referenceConvertMethodModel1.ReturnValue!.Type.Entity);
        Assert.Equal(1, referenceConvertMethodModel1.Parameters.Count);
        Assert.Equal(referenceClassMyModel, referenceConvertMethodModel1.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, referenceConvertMethodModel1.Parameters[0].Modifier);
        Assert.Null(referenceConvertMethodModel1.Parameters[0].DefaultValue);
        Assert.Equal(1, referenceConvertMethodModel1.OutgoingCalls.Count);
        Assert.Equal(referenceCreateMethodModel, referenceConvertMethodModel1.OutgoingCalls[0].Caller);

        Assert.Equal("Convert", referenceConvertMethodModel2.Name);
        Assert.Equal(referenceClassCreateService, referenceConvertMethodModel2.Entity);
        Assert.Equal("", referenceConvertMethodModel2.Modifier);
        Assert.Equal(AccessModifier.Public, referenceConvertMethodModel2.AccessModifier);
        Assert.NotNull(referenceConvertMethodModel2.ReturnValue);
        Assert.Equal(referenceClassMyModel, referenceConvertMethodModel2.ReturnValue!.Type.Entity);
        Assert.Equal(1, referenceConvertMethodModel2.Parameters.Count);
        Assert.Equal(referenceClassOtherModel, referenceConvertMethodModel2.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, referenceConvertMethodModel2.Parameters[0].Modifier);
        Assert.Null(referenceConvertMethodModel2.Parameters[0].DefaultValue);
        Assert.Equal(1, referenceConvertMethodModel2.OutgoingCalls.Count);
        Assert.Equal(referenceConvertMethodModel1, referenceConvertMethodModel2.OutgoingCalls[0].Caller);

        Assert.Equal("Process", referenceProcessMethodModel.Name);
        Assert.Equal(referenceClassCreateService, referenceProcessMethodModel.Entity);
        Assert.Equal("", referenceProcessMethodModel.Modifier);
        Assert.Equal(AccessModifier.Public, referenceProcessMethodModel.AccessModifier);
        Assert.NotNull(referenceProcessMethodModel.ReturnValue);
        Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.ReturnValue!.Type.Entity);
        Assert.Equal(2, referenceProcessMethodModel.Parameters.Count);
        Assert.Equal(referenceClassMyModel, referenceProcessMethodModel.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, referenceProcessMethodModel.Parameters[0].Modifier);
        Assert.Null(referenceProcessMethodModel.Parameters[0].DefaultValue);
        Assert.Equal(2, referenceProcessMethodModel.OutgoingCalls.Count);
        Assert.Equal(referenceCreateMethodModel, referenceProcessMethodModel.OutgoingCalls[0].Caller);
        Assert.Equal(referenceConvertMethodModel2, referenceProcessMethodModel.OutgoingCalls[1].Caller);


        Assert.Equal("Project1.Models", referenceNamespaceModels.FilePath);
        Assert.Equal(projectModel1, referenceNamespaceModels.Project);
        Assert.Equal(2, referenceNamespaceModels.Entities.Count);

        Assert.Equal("Project1.Models.MyModel", referenceClassMyModel.Name);
        Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassMyModel.FilePath);
        Assert.Equal(referenceNamespaceModels, referenceClassMyModel.File);

        Assert.Equal("Project1.Models.OtherModel", referenceClassOtherModel.Name);
        Assert.Equal("validPathToProject/Project1/Models/OtherModel.cs", referenceClassOtherModel.FilePath);
        Assert.Equal(referenceNamespaceModels, referenceClassOtherModel.File);
    }

    [Theory]
    [FileData("TestData/Processors/ReferenceOfClassWithMethodWithPrimitiveTypes.txt")]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithPrimitiveTypesAsParameters_UsingCSharpClassFactExtractor(
            string fileContent)
    {
        var compositeVisitor = new CompositeVisitor();

        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new BaseTypesClassVisitor(),
            new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
            {
                new MethodInfoVisitor(),
                new CalledMethodSetterVisitor(new List<ICSharpMethodCallVisitor>
                {
                    new MethodCallInfoVisitor()
                }),
                new ParameterSetterVisitor(new List<IParameterVisitor>
                {
                    new ParameterInfoVisitor()
                })
            })
        }));

        Mock<ILogger> loggerMock = new();

        compositeVisitor.Accept(new LoggerSetterVisitor(loggerMock.Object));

        var extractor = new CSharpFactExtractor(compositeVisitor);

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classModels = extractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var solutionModel = new RepositoryModel
        {
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    CompilationUnits =
                    {
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Services",
                            ClassTypes = classModels,
                        }
                    }
                }
            }
        };


        var referenceSolutionModel = _sut.Process(solutionModel);

        Assert.Equal(1, referenceSolutionModel.Projects.Count);
        var allCreatedReferences = referenceSolutionModel.CreatedClasses;
        Assert.Equal(5, allCreatedReferences.Count);

        var objectClassModel = allCreatedReferences[0];
        var floatClassModel = allCreatedReferences[1];
        var intClassModel = allCreatedReferences[2];
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
        Assert.Equal(stringClassModel, intParseMethodReference.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, intParseMethodReference.Parameters[0].Modifier);
        Assert.Null(intParseMethodReference.Parameters[0].DefaultValue);

        var projectModel1 = referenceSolutionModel.Projects[0];

        var referenceNamespaceServices = projectModel1.Files[0];
        var referenceMyClass = referenceNamespaceServices.Entities[0] as HoneydewScriptBeePlugin.Models.ClassModel;

        Assert.Equal(1, referenceNamespaceServices.Entities.Count);

        Assert.NotNull(referenceMyClass);
        Assert.Equal("Project1.Services.MyClass", referenceMyClass!.Name);
        Assert.Equal(referenceNamespaceServices, referenceMyClass.File);
        Assert.Empty(referenceMyClass.Fields);
        Assert.Equal(5, referenceMyClass.Methods.Count);

        var methodFunction1 = referenceMyClass.Methods[0];
        var methodFunction2 = referenceMyClass.Methods[1];
        var methodFunction3 = referenceMyClass.Methods[2];
        var methodPrint1 = referenceMyClass.Methods[3];
        var methodPrint2 = referenceMyClass.Methods[4];

        Assert.Equal("Function1", methodFunction1.Name);
        Assert.Equal(referenceMyClass, methodFunction1.Entity);
        Assert.Equal("", methodFunction1.Modifier);
        Assert.Equal(AccessModifier.Public, methodFunction1.AccessModifier);
        Assert.NotNull(methodFunction1.ReturnValue);
        Assert.Equal(floatClassModel, methodFunction1.ReturnValue!.Type.Entity);
        Assert.Equal(2, methodFunction1.Parameters.Count);
        Assert.Equal(intClassModel, methodFunction1.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction1.Parameters[0].Modifier);
        Assert.Null(methodFunction1.Parameters[0].DefaultValue);
        Assert.Equal(intClassModel, methodFunction1.Parameters[1].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction1.Parameters[1].Modifier);
        Assert.Null(methodFunction1.Parameters[1].DefaultValue);
        Assert.Equal(5, methodFunction1.OutgoingCalls.Count);
        Assert.Equal(methodFunction3, methodFunction1.OutgoingCalls[0].Caller);
        Assert.Equal(methodFunction3, methodFunction1.OutgoingCalls[1].Caller);
        Assert.Equal(methodFunction2, methodFunction1.OutgoingCalls[2].Caller);
        Assert.Equal(methodFunction2, methodFunction1.OutgoingCalls[3].Caller);
        Assert.Equal(methodPrint2, methodFunction1.OutgoingCalls[4].Caller);

        Assert.Equal("Function2", methodFunction2.Name);
        Assert.Equal(referenceMyClass, methodFunction2.Entity);
        Assert.Equal("", methodFunction2.Modifier);
        Assert.Equal(AccessModifier.Public, methodFunction2.AccessModifier);
        Assert.NotNull(methodFunction2.ReturnValue);
        Assert.Equal(intClassModel, methodFunction2.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodFunction2.Parameters.Count);
        Assert.Equal(stringClassModel, methodFunction2.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction2.Parameters[0].Modifier);
        Assert.Null(methodFunction2.Parameters[0].DefaultValue);
        Assert.Equal(1, methodFunction2.OutgoingCalls.Count);
        Assert.Equal(intParseMethodReference, methodFunction2.OutgoingCalls[0].Caller);

        Assert.Equal("Function3", methodFunction3.Name);
        Assert.Equal(referenceMyClass, methodFunction3.Entity);
        Assert.Equal("", methodFunction3.Modifier);
        Assert.Equal(AccessModifier.Public, methodFunction3.AccessModifier);
        Assert.NotNull(methodFunction3.ReturnValue);
        Assert.Equal(stringClassModel, methodFunction3.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodFunction3.Parameters.Count);
        Assert.Equal(intClassModel, methodFunction3.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodFunction3.Parameters[0].Modifier);
        Assert.Null(methodFunction3.Parameters[0].DefaultValue);
        Assert.Equal(1, methodFunction3.OutgoingCalls.Count);
        Assert.Equal(intToStringReferenceMethod, methodFunction3.OutgoingCalls[0].Caller);

        Assert.Equal("Print", methodPrint1.Name);
        Assert.Equal(referenceMyClass, methodPrint1.Entity);
        Assert.Equal("static", methodPrint1.Modifier);
        Assert.Equal(AccessModifier.Private, methodPrint1.AccessModifier);
        Assert.NotNull(methodPrint1.ReturnValue);
        Assert.Equal(voidClassModel, methodPrint1.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodPrint1.Parameters.Count);
        Assert.Equal(floatClassModel, methodPrint1.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodPrint1.Parameters[0].Modifier);
        Assert.Null(methodPrint1.Parameters[0].DefaultValue);
        Assert.Empty(methodPrint1.OutgoingCalls);

        Assert.Equal("Print", methodPrint2.Name);
        Assert.Equal(referenceMyClass, methodPrint2.Entity);
        Assert.Equal("", methodPrint2.Modifier);
        Assert.Equal(AccessModifier.Private, methodPrint2.AccessModifier);
        Assert.NotNull(methodPrint2.ReturnValue);
        Assert.Equal(voidClassModel, methodPrint2.ReturnValue!.Type.Entity);
        Assert.Equal(1, methodPrint2.Parameters.Count);
        Assert.Equal(intClassModel, methodPrint2.Parameters[0].Type.Entity);
        Assert.Equal(ParameterModifier.None, methodPrint2.Parameters[0].Modifier);
        Assert.Null(methodPrint2.Parameters[0].DefaultValue);
        Assert.Equal(1, methodPrint2.OutgoingCalls.Count);
        Assert.Equal(methodPrint2, methodPrint2.OutgoingCalls[0].Caller);
    }

    [Fact]
    public void
        GetFunction_ShouldReturnReferenceSolutionModelWithFieldReferences_WhenGivenASolutionModelWithClassesWithFieldReferences()
    {
        var solutionModel = new RepositoryModel
        {
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    CompilationUnits =
                    {
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Services",
                            ClassTypes = new List<IClassType>
                            {
                                new ClassModel
                                {
                                    Name = "Project1.Services.CreateService",
                                    FilePath = "validPathToProject/Project1/Services/CreateService.cs",
                                    ContainingNamespaceName = "Project1.Services",
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
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Models",
                            ClassTypes = new List<IClassType>
                            {
                                new ClassModel
                                {
                                    Name = "Project1.Models.MyModel",
                                    FilePath = "validPathToProject/Project1/Models/MyModel.cs",
                                    ContainingNamespaceName = "Project1.Models",
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
                                new ClassModel
                                {
                                    Name = "Project1.Models.OtherModel",
                                    FilePath = "validPathToProject/Project1/Models/OtherModel.cs",
                                    ContainingNamespaceName = "Project1.Models",
                                }
                            }
                        }
                    }
                }
            }
        };

        var referenceRepositoryModel = _sut.Process(solutionModel);

        Assert.Equal(1, referenceRepositoryModel.Projects.Count);
        var allCreatedReferences = referenceRepositoryModel.CreatedClasses;
        Assert.Equal(2, allCreatedReferences.Count);

        var objectClassModel = allCreatedReferences[0];
        var intClassModel = allCreatedReferences[1];

        Assert.Equal("int", intClassModel.Name);
        Assert.Equal("object", objectClassModel.Name);

        var projectModel1 = referenceRepositoryModel.Projects[0];

        var referenceNamespaceServices = projectModel1.Files[0];
        var referenceClassCreateService = referenceNamespaceServices.Entities[0] as HoneydewScriptBeePlugin.Models.ClassModel;

        var referenceNamespaceModels = projectModel1.Files[1];
        var referenceClassMyModel = referenceNamespaceModels.Entities[0] as HoneydewScriptBeePlugin.Models.ClassModel;
        var referenceClassOtherModel = referenceNamespaceModels.Entities[1] as HoneydewScriptBeePlugin.Models.ClassModel;

        Assert.NotNull(referenceClassCreateService);
        Assert.Equal("Project1.Services.CreateService", referenceClassCreateService!.Name);
        Assert.Equal("validPathToProject/Project1/Services/CreateService.cs", referenceClassCreateService.FilePath);
        Assert.Equal(referenceNamespaceServices, referenceClassCreateService.File);
        Assert.Empty(referenceClassCreateService.Methods);
        Assert.Equal(1, referenceClassCreateService.Fields.Count);

        var createServiceClassField = referenceClassCreateService.Fields[0];
        Assert.Equal("Model", createServiceClassField.Name);
        Assert.Equal(referenceClassMyModel, createServiceClassField.Type.Entity);
        Assert.Equal(referenceClassCreateService, createServiceClassField.Entity);
        Assert.Equal("", createServiceClassField.Modifier);
        Assert.Equal(AccessModifier.Private, createServiceClassField.AccessModifier);
        Assert.False(createServiceClassField.IsEvent);

        Assert.NotNull(referenceClassMyModel);
        Assert.Equal("Project1.Models.MyModel", referenceClassMyModel!.Name);
        Assert.Equal("validPathToProject/Project1/Models/MyModel.cs", referenceClassMyModel.FilePath);
        Assert.Equal(referenceNamespaceModels, referenceClassMyModel.File);
        Assert.Empty(referenceClassMyModel.Methods);
        Assert.Equal(2, referenceClassMyModel.Fields.Count);

        var valueFieldModel = referenceClassMyModel.Fields[0];
        Assert.Equal("_value", valueFieldModel.Name);
        Assert.Equal(intClassModel, valueFieldModel.Type.Entity);
        Assert.Equal(referenceClassMyModel, valueFieldModel.Entity);
        Assert.Equal("readonly", valueFieldModel.Modifier);
        Assert.Equal(AccessModifier.Private, valueFieldModel.AccessModifier);
        Assert.False(valueFieldModel.IsEvent);

        var valueEventFieldModel = referenceClassMyModel.Fields[1];
        Assert.Equal("ValueEvent", valueEventFieldModel.Name);
        Assert.Equal(intClassModel, valueEventFieldModel.Type.Entity);
        Assert.Equal(referenceClassMyModel, valueEventFieldModel.Entity);
        Assert.Equal("", valueEventFieldModel.Modifier);
        Assert.Equal(AccessModifier.Public, valueEventFieldModel.AccessModifier);
        Assert.True(valueEventFieldModel.IsEvent);

        Assert.NotNull(referenceClassOtherModel);
        Assert.Equal("Project1.Models.OtherModel", referenceClassOtherModel!.Name);
        Assert.Equal("validPathToProject/Project1/Models/OtherModel.cs", referenceClassOtherModel.FilePath);
        Assert.Equal(referenceNamespaceModels, referenceClassOtherModel.File);
        Assert.Empty(referenceClassOtherModel.Fields);
        Assert.Empty(referenceClassOtherModel.Methods);
    }
}
