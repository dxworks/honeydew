using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Moq;
using Xunit;
using MethodModel = HoneydewModels.Reference.MethodModel;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class RepositoryModelToReferenceRepositoryModelProcessorTestsWithMissingReferencesHandler
    {
        private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

        private readonly CSharpFactExtractor _extractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public RepositoryModelToReferenceRepositoryModelProcessorTestsWithMissingReferencesHandler()
        {
            _sut = new RepositoryModelToReferenceRepositoryModelProcessor();

            var compositeVisitor = new CompositeVisitor();

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });
            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor()
            });
            var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                {
                    calledMethodSetterVisitor
                }),
                calledMethodSetterVisitor
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    localFunctionsSetterClassVisitor
                }),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    localFunctionsSetterClassVisitor
                }),
                new FieldSetterClassVisitor(new List<ICSharpFieldVisitor>(
                    new List<ICSharpFieldVisitor>
                    {
                        new FieldInfoVisitor()
                    })),
                new PropertySetterClassVisitor(new List<IPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                    {
                        new MethodInfoVisitor(),
                        calledMethodSetterVisitor,
                        localFunctionsSetterClassVisitor
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _extractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [FileData("TestData/Processors/ReferenceOfClassWithMethodWithPrimitiveTypes.txt")]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithPrimitiveTypesAsParameters_UsingCSharpFactExtractor(
                string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
                                ClassTypes = classTypes
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(repositoryModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClasses;
            Assert.Equal(5, allCreatedReferences.Count);

            var objectClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "object");
            var intClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "int");
            var stringClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "string");
            var floatClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "float");
            var voidClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "void");

            Assert.NotNull(objectClassModel);
            Assert.NotNull(intClassModel);
            Assert.NotNull(stringClassModel);
            Assert.NotNull(floatClassModel);
            Assert.NotNull(voidClassModel);

            Assert.Equal("object", objectClassModel.Type.Name);
            Assert.Equal("int", intClassModel.Type.Name);
            Assert.Equal("string", stringClassModel.Type.Name);
            Assert.Equal("float", floatClassModel.Type.Name);
            Assert.Equal("void", voidClassModel.Type.Name);


            Assert.Equal(2, intClassModel.Methods.Count);

            var toStringMethodModel = intClassModel.Methods.First(model => model.Name == "ToString");
            var parseMethodModel = intClassModel.Methods.First(model => model.Name == "Parse");

            Assert.Equal("ToString", toStringMethodModel.Name);
            Assert.Equal(intClassModel, toStringMethodModel.ContainingType);
            Assert.Empty(toStringMethodModel.Parameters);

            Assert.Equal("Parse", parseMethodModel.Name);
            Assert.Equal(intClassModel, parseMethodModel.ContainingType);
            Assert.Equal(1, parseMethodModel.Parameters.Count);
            Assert.Equal(stringClassModel, parseMethodModel.Parameters[0].Type.TypeReference);
            Assert.Equal("", parseMethodModel.Parameters[0].Modifier);
            Assert.Null(parseMethodModel.Parameters[0].DefaultValue);

            var compilationUnitModelServices = referenceSolutionModel.Projects[0].Files[0];
            var referenceMyClass = compilationUnitModelServices.Classes[0];

            Assert.Equal(1, compilationUnitModelServices.Classes.Count);

            Assert.Equal("Project1.Services.MyClass", referenceMyClass.Type.Name);
            Assert.Equal(compilationUnitModelServices, referenceMyClass.File);
            Assert.Empty(referenceMyClass.Fields);
            Assert.Equal(5, referenceMyClass.Methods.Count);

            var methodFunction1 = referenceMyClass.Methods[0];
            var methodFunction2 = referenceMyClass.Methods[1];
            var methodFunction3 = referenceMyClass.Methods[2];
            var methodPrint1 = referenceMyClass.Methods[3];
            var methodPrint2 = referenceMyClass.Methods[4];

            Assert.Equal("Function1", methodFunction1.Name);
            Assert.Equal(referenceMyClass, methodFunction1.ContainingType);
            Assert.Equal("", methodFunction1.Modifier);
            Assert.Equal("public", methodFunction1.AccessModifier);
            Assert.Equal(floatClassModel, methodFunction1.ReturnValue.Type.TypeReference);
            Assert.Equal(2, methodFunction1.Parameters.Count);
            Assert.Equal(intClassModel, methodFunction1.Parameters[0].Type.TypeReference);
            Assert.Equal("", methodFunction1.Parameters[0].Modifier);
            Assert.Null(methodFunction1.Parameters[0].DefaultValue);
            Assert.Equal(intClassModel, methodFunction1.Parameters[1].Type.TypeReference);
            Assert.Equal("", methodFunction1.Parameters[1].Modifier);
            Assert.Null(methodFunction1.Parameters[1].DefaultValue);
            Assert.Equal(5, methodFunction1.CalledMethods.Count);
            Assert.Equal(methodFunction3, methodFunction1.CalledMethods[0]);
            Assert.Equal(methodFunction3, methodFunction1.CalledMethods[1]);
            Assert.Equal(methodFunction2, methodFunction1.CalledMethods[2]);
            Assert.Equal(methodFunction2, methodFunction1.CalledMethods[3]);
            Assert.Equal(methodPrint2, methodFunction1.CalledMethods[4]);

            Assert.Equal("Function2", methodFunction2.Name);
            Assert.Equal(referenceMyClass, methodFunction2.ContainingType);
            Assert.Equal("", methodFunction2.Modifier);
            Assert.Equal("public", methodFunction2.AccessModifier);
            Assert.Equal(intClassModel, methodFunction2.ReturnValue.Type.TypeReference);
            Assert.Equal(1, methodFunction2.Parameters.Count);
            Assert.Equal(stringClassModel, methodFunction2.Parameters[0].Type.TypeReference);
            Assert.Equal("", methodFunction2.Parameters[0].Modifier);
            Assert.Null(methodFunction2.Parameters[0].DefaultValue);
            Assert.Equal(1, methodFunction2.CalledMethods.Count);
            Assert.Equal(parseMethodModel, methodFunction2.CalledMethods[0]);

            Assert.Equal("Function3", methodFunction3.Name);
            Assert.Equal(referenceMyClass, methodFunction3.ContainingType);
            Assert.Equal("", methodFunction3.Modifier);
            Assert.Equal("public", methodFunction3.AccessModifier);
            Assert.Equal(stringClassModel, methodFunction3.ReturnValue.Type.TypeReference);
            Assert.Equal(1, methodFunction3.Parameters.Count);
            Assert.Equal(intClassModel, methodFunction3.Parameters[0].Type.TypeReference);
            Assert.Equal("", methodFunction3.Parameters[0].Modifier);
            Assert.Null(methodFunction3.Parameters[0].DefaultValue);
            Assert.Equal(1, methodFunction3.CalledMethods.Count);
            Assert.Equal(toStringMethodModel, methodFunction3.CalledMethods[0]);

            Assert.Equal("Print", methodPrint1.Name);
            Assert.Equal(referenceMyClass, methodPrint1.ContainingType);
            Assert.Equal("static", methodPrint1.Modifier);
            Assert.Equal("private", methodPrint1.AccessModifier);
            Assert.Equal(voidClassModel, methodPrint1.ReturnValue.Type.TypeReference);
            Assert.Equal(1, methodPrint1.Parameters.Count);
            Assert.Equal(floatClassModel, methodPrint1.Parameters[0].Type.TypeReference);
            Assert.Equal("", methodPrint1.Parameters[0].Modifier);
            Assert.Null(methodPrint1.Parameters[0].DefaultValue);
            Assert.Empty(methodPrint1.CalledMethods);

            Assert.Equal("Print", methodPrint2.Name);
            Assert.Equal(referenceMyClass, methodPrint2.ContainingType);
            Assert.Equal("", methodPrint2.Modifier);
            Assert.Equal("private", methodPrint2.AccessModifier);
            Assert.Equal(voidClassModel, methodPrint2.ReturnValue.Type.TypeReference);
            Assert.Equal(1, methodPrint2.Parameters.Count);
            Assert.Equal(intClassModel, methodPrint2.Parameters[0].Type.TypeReference);
            Assert.Equal("", methodPrint2.Parameters[0].Modifier);
            Assert.Null(methodPrint2.Parameters[0].DefaultValue);
            Assert.Equal(1, methodPrint2.CalledMethods.Count);
            Assert.Equal(methodPrint2, methodPrint2.CalledMethods[0]);
        }

        [Theory]
        [FileData("TestData/Processors/ReferenceOfClassWithMethodWithNumericValuesAsParameters.txt")]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithNumericValesAsParameters_UsingCSharpFactExtractor(
                string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
                                ClassTypes = classTypes,
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(repositoryModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClasses;
            Assert.Equal(6, allCreatedReferences.Count);

            var objectClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "object");
            var intClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "int");
            var shortClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "short");
            var longClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "long");
            var byteClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "byte");
            var voidClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "void");

            Assert.NotNull(objectClassModel);
            Assert.NotNull(intClassModel);
            Assert.NotNull(shortClassModel);
            Assert.NotNull(longClassModel);
            Assert.NotNull(byteClassModel);
            Assert.NotNull(voidClassModel);

            Assert.Equal("object", objectClassModel.Name);
            Assert.Equal("int", intClassModel.Name);
            Assert.Equal("short", shortClassModel.Name);
            Assert.Equal("long", longClassModel.Name);
            Assert.Equal("byte", byteClassModel.Name);
            Assert.Equal("void", voidClassModel.Name);

            var referenceCompilationUnitServices = referenceSolutionModel.Projects[0].Files[0];
            var referenceMyClass = referenceCompilationUnitServices.Classes[0];

            Assert.Equal(1, referenceCompilationUnitServices.Classes.Count);

            Assert.Equal("Project1.Services.MyClass", referenceMyClass.Type.Name);
            Assert.Equal(referenceCompilationUnitServices, referenceMyClass.File);
            Assert.Empty(referenceMyClass.Fields);
            Assert.Equal(5, referenceMyClass.Methods.Count);

            var printNoArg = referenceMyClass.Methods[0];
            var printInt = referenceMyClass.Methods[1];
            var printShort = referenceMyClass.Methods[2];
            var printLong = referenceMyClass.Methods[3];
            var printByte = referenceMyClass.Methods[4];

            Assert.Equal("Print", printNoArg.Name);
            Assert.Equal(referenceMyClass, printNoArg.ContainingType);
            Assert.Equal("", printNoArg.Modifier);
            Assert.Equal("public", printNoArg.AccessModifier);
            Assert.Equal(voidClassModel, printNoArg.ReturnValue.Type.TypeReference);
            Assert.Empty(printNoArg.Parameters);
            Assert.Equal(4, printNoArg.CalledMethods.Count);
            Assert.Equal(printInt, printNoArg.CalledMethods[0]);
            Assert.Equal(printLong, printNoArg.CalledMethods[1]);
            Assert.Equal(printShort, printNoArg.CalledMethods[2]);
            Assert.Equal(printByte, printNoArg.CalledMethods[3]);

            Assert.Equal("Print", printInt.Name);
            Assert.Equal(referenceMyClass, printInt.ContainingType);
            Assert.Equal("", printInt.Modifier);
            Assert.Equal("private", printInt.AccessModifier);
            Assert.Equal(voidClassModel, printInt.ReturnValue.Type.TypeReference);
            Assert.Equal(1, printInt.Parameters.Count);
            Assert.Equal(intClassModel, printInt.Parameters[0].Type.TypeReference);
            Assert.Equal("", printInt.Parameters[0].Modifier);
            Assert.Null(printInt.Parameters[0].DefaultValue);
            Assert.Empty(printInt.CalledMethods);

            Assert.Equal("Print", printShort.Name);
            Assert.Equal(referenceMyClass, printShort.ContainingType);
            Assert.Equal("", printShort.Modifier);
            Assert.Equal("private", printShort.AccessModifier);
            Assert.Equal(voidClassModel, printShort.ReturnValue.Type.TypeReference);
            Assert.Equal(1, printShort.Parameters.Count);
            Assert.Equal(shortClassModel, printShort.Parameters[0].Type.TypeReference);
            Assert.Equal("", printShort.Parameters[0].Modifier);
            Assert.Null(printShort.Parameters[0].DefaultValue);
            Assert.Empty(printShort.CalledMethods);

            Assert.Equal("Print", printLong.Name);
            Assert.Equal(referenceMyClass, printLong.ContainingType);
            Assert.Equal("", printLong.Modifier);
            Assert.Equal("private", printLong.AccessModifier);
            Assert.Equal(voidClassModel, printLong.ReturnValue.Type.TypeReference);
            Assert.Equal(1, printLong.Parameters.Count);
            Assert.Equal(longClassModel, printLong.Parameters[0].Type.TypeReference);
            Assert.Equal("", printLong.Parameters[0].Modifier);
            Assert.Null(printLong.Parameters[0].DefaultValue);
            Assert.Empty(printLong.CalledMethods);

            Assert.Equal("Print", printByte.Name);
            Assert.Equal(referenceMyClass, printByte.ContainingType);
            Assert.Equal("", printByte.Modifier);
            Assert.Equal("private", printByte.AccessModifier);
            Assert.Equal(voidClassModel, printByte.ReturnValue.Type.TypeReference);
            Assert.Equal(1, printByte.Parameters.Count);
            Assert.Equal(byteClassModel, printByte.Parameters[0].Type.TypeReference);
            Assert.Equal("", printByte.Parameters[0].Modifier);
            Assert.Null(printByte.Parameters[0].DefaultValue);
            Assert.Empty(printByte.CalledMethods);
        }

        [Theory]
        [FileData("TestData/Processors/ReferenceOfNamespaceWithMultipleClasses.txt")]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllClassReferences_WhenGivenASolutionModelWithClassHierarchy_UsingCSharpFactExtractor(
                string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
                                ClassTypes = classTypes,
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(repositoryModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClasses;
            Assert.Equal(1, allCreatedReferences.Count);

            var objectClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "object");

            Assert.NotNull(objectClassModel);
            Assert.Equal("object", objectClassModel.Name);

            var referenceNamespaceServices = referenceSolutionModel.Projects[0].Files[0];

            var referenceIInterface = referenceNamespaceServices.Classes[0];
            var referenceMyInterface = referenceNamespaceServices.Classes[1];
            var referenceOtherInterface = referenceNamespaceServices.Classes[2];
            var referenceBaseClass = referenceNamespaceServices.Classes[3];
            var referenceChildClass1 = referenceNamespaceServices.Classes[4];
            var referenceChildClass2 = referenceNamespaceServices.Classes[5];
            var referenceModel = referenceNamespaceServices.Classes[6];
            var referenceChildClass3 = referenceNamespaceServices.Classes[7];

            Assert.Equal(8, referenceNamespaceServices.Classes.Count);

            Assert.Equal("Project1.MyNamespace.IInterface", referenceIInterface.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceIInterface.File);
            Assert.Empty(referenceIInterface.BaseTypes);

            Assert.Equal("Project1.MyNamespace.MyInterface", referenceMyInterface.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceMyInterface.File);
            Assert.Equal("interface", referenceMyInterface.ClassType);
            Assert.Equal("public", referenceMyInterface.AccessModifier);
            Assert.Equal("", referenceMyInterface.Modifier);
            Assert.Equal(1, referenceMyInterface.BaseTypes.Count);
            Assert.Equal(referenceIInterface, referenceMyInterface.BaseTypes[0]);

            Assert.Equal("Project1.MyNamespace.OtherInterface", referenceOtherInterface.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceOtherInterface.File);
            Assert.Equal("interface", referenceOtherInterface.ClassType);
            Assert.Equal("public", referenceOtherInterface.AccessModifier);
            Assert.Equal("", referenceOtherInterface.Modifier);
            Assert.Empty(referenceOtherInterface.BaseTypes);

            Assert.Equal("Project1.MyNamespace.BaseClass", referenceBaseClass.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceBaseClass.File);
            Assert.Equal("class", referenceBaseClass.ClassType);
            Assert.Equal("public", referenceBaseClass.AccessModifier);
            Assert.Equal("", referenceBaseClass.Modifier);
            Assert.Equal(1, referenceBaseClass.BaseTypes.Count);
            Assert.Equal(objectClassModel, referenceBaseClass.BaseTypes[0]);

            Assert.Equal("Project1.MyNamespace.ChildClass1", referenceChildClass1.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceChildClass1.File);
            Assert.Equal("class", referenceChildClass1.ClassType);
            Assert.Equal("public", referenceChildClass1.AccessModifier);
            Assert.Equal("", referenceChildClass1.Modifier);
            Assert.Equal(2, referenceChildClass1.BaseTypes.Count);
            Assert.Equal(referenceBaseClass, referenceChildClass1.BaseTypes[0]);
            Assert.Equal(referenceIInterface, referenceChildClass1.BaseTypes[1]);

            Assert.Equal("Project1.MyNamespace.ChildClass2", referenceChildClass2.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceChildClass2.File);
            Assert.Equal("class", referenceChildClass2.ClassType);
            Assert.Equal("public", referenceChildClass2.AccessModifier);
            Assert.Equal("", referenceChildClass2.Modifier);
            Assert.Equal(3, referenceChildClass2.BaseTypes.Count);
            Assert.Equal(referenceBaseClass, referenceChildClass2.BaseTypes[0]);
            Assert.Equal(referenceMyInterface, referenceChildClass2.BaseTypes[1]);
            Assert.Equal(referenceOtherInterface, referenceChildClass2.BaseTypes[2]);

            Assert.Equal("Project1.MyNamespace.Model", referenceModel.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceModel.File);
            Assert.Equal("class", referenceModel.ClassType);
            Assert.Equal("public", referenceModel.AccessModifier);
            Assert.Equal("", referenceModel.Modifier);
            Assert.Equal(2, referenceModel.BaseTypes.Count);
            Assert.Equal(objectClassModel, referenceModel.BaseTypes[0]);
            Assert.Equal(referenceOtherInterface, referenceModel.BaseTypes[1]);

            Assert.Equal("Project1.MyNamespace.ChildClass3", referenceChildClass3.Type.Name);
            Assert.Equal(referenceNamespaceServices, referenceChildClass3.File);
            Assert.Equal("class", referenceChildClass3.ClassType);
            Assert.Equal("public", referenceChildClass3.AccessModifier);
            Assert.Equal("", referenceChildClass3.Modifier);
            Assert.Equal(1, referenceChildClass3.BaseTypes.Count);
            Assert.Equal(referenceChildClass1, referenceChildClass3.BaseTypes[0]);
        }

        [Theory]
        [FileData("TestData/Processors/ReferenceWithClassHierarchy.txt")]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesWithClassHierarchyAsParameter_UsingCSharpFactExtractor(
                string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
                                FilePath = "Project1.MyNamespace",
                                ClassTypes = classTypes,
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClasses;
            Assert.Equal(4, allCreatedReferences.Count);

            var objectClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "object");
            var intClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "int");
            var floatClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "float");

            var voidClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "void");

            Assert.NotNull(objectClassModel);
            Assert.NotNull(intClassModel);
            Assert.NotNull(floatClassModel);
            Assert.NotNull(voidClassModel);

            Assert.Equal("object", objectClassModel.Type.Name);
            Assert.Equal("int", intClassModel.Type.Name);
            Assert.Equal("float", floatClassModel.Type.Name);
            Assert.Equal("void", voidClassModel.Type.Name);

            var referenceNamespaceServices = referenceSolutionModel.Projects[0].Files[0];
            var baseClass = referenceNamespaceServices.Classes[0];
            var childClass1 = referenceNamespaceServices.Classes[1];
            var childClass2 = referenceNamespaceServices.Classes[2];
            var modelClass = referenceNamespaceServices.Classes[3];
            var childClass3 = referenceNamespaceServices.Classes[4];
            var callerClass = referenceNamespaceServices.Classes[5];

            Assert.Equal(6, referenceNamespaceServices.Classes.Count);

            Assert.Equal("Project1.MyNamespace.BaseClass", baseClass.Type.Name);
            Assert.Equal(referenceNamespaceServices, baseClass.File);
            Assert.Equal(1, baseClass.Constructors.Count);
            Assert.Equal("BaseClass", baseClass.Constructors[0].Name);
            Assert.Equal("", baseClass.Constructors[0].Modifier);
            Assert.Equal("public", baseClass.Constructors[0].AccessModifier);
            Assert.Empty(baseClass.Constructors[0].CalledMethods);
            Assert.Equal(baseClass, baseClass.Constructors[0].Class);
            Assert.Empty(baseClass.Constructors[0].Parameters);
            Assert.Equal(1, baseClass.Methods.Count);
            Assert.Equal(1, baseClass.Constructors.Count);
            Assert.True(baseClass.Methods[0].IsConstructor);
            Assert.Empty(baseClass.Metrics);
            Assert.Equal(1, baseClass.Fields.Count);

            var baseClassFieldX = baseClass.Fields[0];
            Assert.Equal("X", baseClassFieldX.Name);
            Assert.Equal(baseClass, baseClassFieldX.Class);
            Assert.Equal(intClassModel, baseClassFieldX.Type.TypeReference);
            Assert.Equal("", baseClassFieldX.Modifier);
            Assert.Equal("public", baseClassFieldX.AccessModifier);
            Assert.False(baseClassFieldX.IsEvent);


            Assert.Equal("Project1.MyNamespace.ChildClass1", childClass1.Type.Name);
            Assert.Equal(referenceNamespaceServices, childClass1.File);
            Assert.Empty(childClass1.Methods);
            Assert.Empty(childClass1.Metrics);
            Assert.Empty(childClass1.Fields);


            Assert.Equal("Project1.MyNamespace.ChildClass2", childClass2.Type.Name);
            Assert.Equal(referenceNamespaceServices, childClass2.File);
            Assert.Empty(childClass2.Methods);
            Assert.Empty(childClass2.Metrics);
            Assert.Equal(1, childClass2.Fields.Count);

            var childClass2FieldZ = childClass2.Fields[0];
            Assert.Equal("Z", childClass2FieldZ.Name);
            Assert.Equal(childClass2, childClass2FieldZ.Class);
            Assert.Equal(floatClassModel, childClass2FieldZ.Type.TypeReference);
            Assert.Equal("", childClass2FieldZ.Modifier);
            Assert.Equal("public", childClass2FieldZ.AccessModifier);
            Assert.False(childClass2FieldZ.IsEvent);


            Assert.Equal("Project1.MyNamespace.Model", modelClass.Type.Name);
            Assert.Equal(referenceNamespaceServices, modelClass.File);
            Assert.Empty(modelClass.Methods);
            Assert.Empty(modelClass.Metrics);
            Assert.Empty(modelClass.Fields);


            Assert.Equal("Project1.MyNamespace.ChildClass3", childClass3.Type.Name);
            Assert.Equal(referenceNamespaceServices, childClass3.File);
            Assert.Empty(childClass3.Methods);
            Assert.Empty(childClass3.Metrics);
            Assert.Equal(1, childClass3.Fields.Count);

            var childClass3ModelField = childClass3.Fields[0];
            Assert.Equal("_model", childClass3ModelField.Name);
            Assert.Equal(childClass3, childClass3ModelField.Class);
            Assert.Equal(modelClass, childClass3ModelField.Type.TypeReference);
            Assert.Equal("readonly", childClass3ModelField.Modifier);
            Assert.Equal("private", childClass3ModelField.AccessModifier);
            Assert.False(childClass3ModelField.IsEvent);


            Assert.Equal("Project1.MyNamespace.Caller", callerClass.Type.Name);
            Assert.Equal(referenceNamespaceServices, callerClass.File);
            Assert.Empty(callerClass.Fields);
            Assert.Empty(callerClass.Metrics);
            Assert.Equal(2, callerClass.Methods.Count);

            var callMethod0 = callerClass.Methods[0];
            Assert.Equal("Call", callMethod0.Name);
            Assert.Equal(callerClass, callMethod0.ContainingType);
            Assert.Equal("", callMethod0.Modifier);
            Assert.Equal("public", callMethod0.AccessModifier);
            Assert.Equal(voidClassModel, callMethod0.ReturnValue.Type.TypeReference);
            Assert.Equal(1, callMethod0.Parameters.Count);
            Assert.Equal(baseClass, callMethod0.Parameters[0].Type.TypeReference);
            Assert.Equal("", callMethod0.Parameters[0].Modifier);
            Assert.Null(callMethod0.Parameters[0].DefaultValue);
            Assert.Empty(callMethod0.CalledMethods);

            var callMethod1 = callerClass.Methods[1];
            Assert.Equal("Call", callMethod1.Name);
            Assert.Equal(callerClass, callMethod1.ContainingType);
            Assert.Equal("static", callMethod1.Modifier);
            Assert.Equal("public", callMethod1.AccessModifier);
            Assert.Equal(voidClassModel, callMethod1.ReturnValue.Type.TypeReference);
            Assert.Empty(callMethod1.Parameters);
            Assert.Equal(6, callMethod1.CalledMethods.Count);

            foreach (var calledMethod in callMethod1.CalledMethods)
            {
                Assert.Equal(callMethod0, calledMethod);
            }
        }

        [Theory]
        [FileData("TestData/Processors/ReferenceOfDeepNestedLocalFunctions.txt")]
        public void Process_ShouldHaveLocalFunctionReferences_WhenGivenMethodWithDeepNestedLocalFunctions(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
                                FilePath = "Project1/class1.cs",
                                ClassTypes = classTypes,
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(repositoryModel);

            var classModel = referenceSolutionModel.Projects[0].Files[0].Classes[0];
            var methodModel = classModel.Methods[0];
            var methodModelLocalFunction = methodModel.LocalFunctions[0];

            Assert.Equal(classModel, methodModel.ContainingType);
            Assert.Equal(1, methodModel.CalledMethods.Count);
            Assert.Equal(methodModelLocalFunction, methodModel.CalledMethods[0]);
            Assert.Equal(methodModel, methodModelLocalFunction.ContainingType);
            Assert.Equal(1, methodModel.LocalFunctions.Count);
            AssertLocalFunctions(methodModelLocalFunction);

            var constructorModel = classModel.Constructors[0];
            var constructorModelLocalFunction = constructorModel.LocalFunctions[0];
            Assert.Equal(classModel, constructorModel.Class);
            Assert.Equal(1, constructorModel.CalledMethods.Count);
            Assert.Equal(constructorModelLocalFunction, constructorModel.CalledMethods[0]);
            Assert.Equal(constructorModel, constructorModelLocalFunction.ContainingType);
            Assert.Equal(1, constructorModel.LocalFunctions.Count);
            AssertLocalFunctions(constructorModelLocalFunction);

            var propertyModel = classModel.Properties[0];
            var getAccessor = propertyModel.Accessors[0];
            var getAccessorModelLocalFunction = getAccessor.LocalFunctions[0];
            Assert.Equal(propertyModel, getAccessor.ContainingType);
            Assert.Equal(1, getAccessor.CalledMethods.Count);
            Assert.Equal(getAccessorModelLocalFunction, getAccessor.CalledMethods[0]);
            Assert.Equal(getAccessor, getAccessorModelLocalFunction.ContainingType);
            Assert.Equal(1, getAccessor.LocalFunctions.Count);
            AssertLocalFunctions(getAccessorModelLocalFunction);

            var setAccessor = propertyModel.Accessors[1];
            var setAccessorModelLocalFunction = setAccessor.LocalFunctions[0];
            Assert.Equal(propertyModel, setAccessor.ContainingType);
            Assert.Equal(1, constructorModel.CalledMethods.Count);
            Assert.Equal(constructorModelLocalFunction, constructorModel.CalledMethods[0]);
            Assert.Equal(setAccessor, setAccessorModelLocalFunction.ContainingType);
            Assert.Equal(1, constructorModel.LocalFunctions.Count);
            AssertLocalFunctions(setAccessorModelLocalFunction);

            void AssertLocalFunctions(MethodModel localFunction1)
            {
                Assert.Equal("LocalFunction1", localFunction1.Name);
                Assert.Equal(1, localFunction1.CalledMethods.Count);

                var localFunction2 = localFunction1.LocalFunctions[0];
                Assert.Equal(localFunction2, localFunction1.CalledMethods[0]);
                Assert.Equal(1, localFunction1.LocalFunctions.Count);
                Assert.Equal("LocalFunction2", localFunction2.Name);
                Assert.Equal(localFunction1, localFunction2.ContainingType);
                Assert.Equal(1, localFunction2.CalledMethods.Count);

                var localFunction3 = localFunction2.LocalFunctions[0];
                Assert.Equal(localFunction3, localFunction2.CalledMethods[0]);
                Assert.Equal(1, localFunction2.LocalFunctions.Count);
                Assert.Equal("LocalFunction3", localFunction3.Name);
                Assert.Equal(localFunction2, localFunction3.ContainingType);
                Assert.Empty(localFunction3.CalledMethods);
            }
        }
    }
}
