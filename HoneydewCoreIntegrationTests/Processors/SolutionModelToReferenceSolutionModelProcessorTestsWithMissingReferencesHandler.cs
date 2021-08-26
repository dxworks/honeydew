using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Constructor;
using HoneydewExtractors.Core.Metrics.Extraction.Field;
using HoneydewExtractors.Core.Metrics.Extraction.Method;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Parameter;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class SolutionModelToReferenceSolutionModelProcessorTestsWithMissingReferencesHandler
    {
        private readonly SolutionModelToReferenceSolutionModelProcessor _sut;

        private readonly CSharpFactExtractor _extractor;

        public SolutionModelToReferenceSolutionModelProcessorTestsWithMissingReferencesHandler()
        {
            _sut = new SolutionModelToReferenceSolutionModelProcessor();

            var compositeVisitor = new CompositeVisitor();

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });
            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor
                }),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    calledMethodSetterVisitor,
                    parameterSetterVisitor
                }),
                new FieldSetterClassVisitor(new List<ICSharpFieldVisitor>(
                    new List<ICSharpFieldVisitor>
                    {
                        new FieldInfoVisitor()
                    }))
            }));

            _extractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithPrimitiveTypesAsParameters_UsingCSharpFactExtractor()
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

            var classModels = _extractor.Extract(fileContent).ClassTypes;

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
                                ClassModels = classModels.Cast<ClassModel>().ToList(),
                            }
                        }
                    }
                }
            };


            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
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

            Assert.Equal("object", objectClassModel.Name);
            Assert.Equal("int", intClassModel.Name);
            Assert.Equal("string", stringClassModel.Name);
            Assert.Equal("float", floatClassModel.Name);
            Assert.Equal("void", voidClassModel.Name);


            Assert.Equal(2, intClassModel.Methods.Count);

            var toStringMethodModel = intClassModel.Methods.First(model => model.Name == "ToString");
            var parseMethodModel = intClassModel.Methods.First(model => model.Name == "Parse");

            Assert.Equal("ToString", toStringMethodModel.Name);
            Assert.Equal(intClassModel, toStringMethodModel.ContainingClass);
            Assert.Empty(toStringMethodModel.Parameters);

            Assert.Equal("Parse", parseMethodModel.Name);
            Assert.Equal(intClassModel, parseMethodModel.ContainingClass);
            Assert.Equal(1, parseMethodModel.Parameters.Count);
            Assert.Equal(stringClassModel, parseMethodModel.Parameters[0].Type);
            Assert.Equal("", parseMethodModel.Parameters[0].Modifier);
            Assert.Null(parseMethodModel.Parameters[0].DefaultValue);

            var referenceNamespaceServices = referenceSolutionModel.Projects[0].Namespaces[0];
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
            Assert.Equal(parseMethodModel, methodFunction2.CalledMethods[0]);

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
            Assert.Equal(toStringMethodModel, methodFunction3.CalledMethods[0]);

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
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithNumericValesAsParameters_UsingCSharpFactExtractor()
        {
            const string fileContent = @"
         namespace Project1.Services
         {
             public class MyClass
             {
                 public void Print()
                 {
                     Print(2);
                     Print(2L);
                     
                     const short a = 2;
                     Print(a);
                     Print((byte)a);
                 }

                 private void Print(int a)
                 {
                 }

                 private void Print(short a)
                 {

                 }

                 private void Print(long a)
                 {

                 }

                 private void Print(byte a)
                 {
                 }
             }
         }";

            var classModels = _extractor.Extract(fileContent).ClassTypes;

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
                                ClassModels = classModels.Cast<ClassModel>().ToList(),
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
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

            var referenceNamespaceServices = referenceSolutionModel.Projects[0].Namespaces[0];
            var referenceMyClass = referenceNamespaceServices.ClassModels[0];

            Assert.Equal(1, referenceNamespaceServices.ClassModels.Count);

            Assert.Equal("Project1.Services.MyClass", referenceMyClass.Name);
            Assert.Equal(referenceNamespaceServices, referenceMyClass.NamespaceReference);
            Assert.Empty(referenceMyClass.Fields);
            Assert.Equal(5, referenceMyClass.Methods.Count);

            var printNoArg = referenceMyClass.Methods[0];
            var printInt = referenceMyClass.Methods[1];
            var printShort = referenceMyClass.Methods[2];
            var printLong = referenceMyClass.Methods[3];
            var printByte = referenceMyClass.Methods[4];

            Assert.Equal("Print", printNoArg.Name);
            Assert.Equal(referenceMyClass, printNoArg.ContainingClass);
            Assert.Equal("", printNoArg.Modifier);
            Assert.Equal("public", printNoArg.AccessModifier);
            Assert.Equal(voidClassModel, printNoArg.ReturnTypeReferenceClassModel);
            Assert.Empty(printNoArg.Parameters);
            Assert.Equal(4, printNoArg.CalledMethods.Count);
            Assert.Equal(printInt, printNoArg.CalledMethods[0]);
            Assert.Equal(printLong, printNoArg.CalledMethods[1]);
            Assert.Equal(printShort, printNoArg.CalledMethods[2]);
            Assert.Equal(printByte, printNoArg.CalledMethods[3]);

            Assert.Equal("Print", printInt.Name);
            Assert.Equal(referenceMyClass, printInt.ContainingClass);
            Assert.Equal("", printInt.Modifier);
            Assert.Equal("private", printInt.AccessModifier);
            Assert.Equal(voidClassModel, printInt.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printInt.Parameters.Count);
            Assert.Equal(intClassModel, printInt.Parameters[0].Type);
            Assert.Equal("", printInt.Parameters[0].Modifier);
            Assert.Null(printInt.Parameters[0].DefaultValue);
            Assert.Empty(printInt.CalledMethods);

            Assert.Equal("Print", printShort.Name);
            Assert.Equal(referenceMyClass, printShort.ContainingClass);
            Assert.Equal("", printShort.Modifier);
            Assert.Equal("private", printShort.AccessModifier);
            Assert.Equal(voidClassModel, printShort.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printShort.Parameters.Count);
            Assert.Equal(shortClassModel, printShort.Parameters[0].Type);
            Assert.Equal("", printShort.Parameters[0].Modifier);
            Assert.Null(printShort.Parameters[0].DefaultValue);
            Assert.Empty(printShort.CalledMethods);

            Assert.Equal("Print", printLong.Name);
            Assert.Equal(referenceMyClass, printLong.ContainingClass);
            Assert.Equal("", printLong.Modifier);
            Assert.Equal("private", printLong.AccessModifier);
            Assert.Equal(voidClassModel, printLong.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printLong.Parameters.Count);
            Assert.Equal(longClassModel, printLong.Parameters[0].Type);
            Assert.Equal("", printLong.Parameters[0].Modifier);
            Assert.Null(printLong.Parameters[0].DefaultValue);
            Assert.Empty(printLong.CalledMethods);

            Assert.Equal("Print", printByte.Name);
            Assert.Equal(referenceMyClass, printByte.ContainingClass);
            Assert.Equal("", printByte.Modifier);
            Assert.Equal("private", printByte.AccessModifier);
            Assert.Equal(voidClassModel, printByte.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printByte.Parameters.Count);
            Assert.Equal(byteClassModel, printByte.Parameters[0].Type);
            Assert.Equal("", printByte.Parameters[0].Modifier);
            Assert.Null(printByte.Parameters[0].DefaultValue);
            Assert.Empty(printByte.CalledMethods);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllClassReferences_WhenGivenASolutionModelWithClassHierarchy_UsingCSharpFactExtractor()
        {
            const string fileContent = @"
          namespace Project1.MyNamespace
          {
              public interface IInterface {}

              public interface MyInterface : IInterface {}

              public interface OtherInterface {}

              public class BaseClass {}

              public class ChildClass1 : BaseClass, IInterface {}

              public class ChildClass2 : BaseClass, MyInterface, OtherInterface {}

              public class Model : OtherInterface {}

              public class ChildClass3 : ChildClass1 {}
          }";

            var classModels = _extractor.Extract(fileContent).ClassTypes;

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
                                ClassModels = classModels.Cast<ClassModel>().ToList(),
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
            Assert.Equal(1, allCreatedReferences.Count);

            var objectClassModel =
                allCreatedReferences.SingleOrDefault(a => a.Name == "object");

            Assert.NotNull(objectClassModel);
            Assert.Equal("object", objectClassModel.Name);

            var referenceNamespaceServices = referenceSolutionModel.Projects[0].Namespaces[0];

            var referenceIInterface = referenceNamespaceServices.ClassModels[0];
            var referenceMyInterface = referenceNamespaceServices.ClassModels[1];
            var referenceOtherInterface = referenceNamespaceServices.ClassModels[2];
            var referenceBaseClass = referenceNamespaceServices.ClassModels[3];
            var referenceChildClass1 = referenceNamespaceServices.ClassModels[4];
            var referenceChildClass2 = referenceNamespaceServices.ClassModels[5];
            var referenceModel = referenceNamespaceServices.ClassModels[6];
            var referenceChildClass3 = referenceNamespaceServices.ClassModels[7];

            Assert.Equal(8, referenceNamespaceServices.ClassModels.Count);

            Assert.Equal("Project1.MyNamespace.IInterface", referenceIInterface.Name);
            Assert.Equal(referenceNamespaceServices, referenceIInterface.NamespaceReference);
            Assert.Null(referenceIInterface.BaseClass);
            Assert.Empty(referenceIInterface.BaseInterfaces);

            Assert.Equal("Project1.MyNamespace.MyInterface", referenceMyInterface.Name);
            Assert.Equal(referenceNamespaceServices, referenceMyInterface.NamespaceReference);
            Assert.Equal("interface", referenceMyInterface.ClassType);
            Assert.Equal("public", referenceMyInterface.AccessModifier);
            Assert.Equal("", referenceMyInterface.Modifier);
            Assert.Null(referenceMyInterface.BaseClass);
            Assert.Equal(1, referenceMyInterface.BaseInterfaces.Count);
            Assert.Equal(referenceIInterface, referenceMyInterface.BaseInterfaces[0]);

            Assert.Equal("Project1.MyNamespace.OtherInterface", referenceOtherInterface.Name);
            Assert.Equal(referenceNamespaceServices, referenceOtherInterface.NamespaceReference);
            Assert.Equal("interface", referenceOtherInterface.ClassType);
            Assert.Equal("public", referenceOtherInterface.AccessModifier);
            Assert.Equal("", referenceOtherInterface.Modifier);
            Assert.Null(referenceOtherInterface.BaseClass);
            Assert.Empty(referenceOtherInterface.BaseInterfaces);

            Assert.Equal("Project1.MyNamespace.BaseClass", referenceBaseClass.Name);
            Assert.Equal(referenceNamespaceServices, referenceBaseClass.NamespaceReference);
            Assert.Equal("class", referenceBaseClass.ClassType);
            Assert.Equal("public", referenceBaseClass.AccessModifier);
            Assert.Equal("", referenceBaseClass.Modifier);
            Assert.Equal(objectClassModel, referenceBaseClass.BaseClass);
            Assert.Empty(referenceBaseClass.BaseInterfaces);

            Assert.Equal("Project1.MyNamespace.ChildClass1", referenceChildClass1.Name);
            Assert.Equal(referenceNamespaceServices, referenceChildClass1.NamespaceReference);
            Assert.Equal("class", referenceChildClass1.ClassType);
            Assert.Equal("public", referenceChildClass1.AccessModifier);
            Assert.Equal("", referenceChildClass1.Modifier);
            Assert.Equal(referenceBaseClass, referenceChildClass1.BaseClass);
            Assert.Equal(1, referenceChildClass1.BaseInterfaces.Count);
            Assert.Equal(referenceIInterface, referenceChildClass1.BaseInterfaces[0]);

            Assert.Equal("Project1.MyNamespace.ChildClass2", referenceChildClass2.Name);
            Assert.Equal(referenceNamespaceServices, referenceChildClass2.NamespaceReference);
            Assert.Equal("class", referenceChildClass2.ClassType);
            Assert.Equal("public", referenceChildClass2.AccessModifier);
            Assert.Equal("", referenceChildClass2.Modifier);
            Assert.Equal(referenceBaseClass, referenceChildClass2.BaseClass);
            Assert.Equal(2, referenceChildClass2.BaseInterfaces.Count);
            Assert.Equal(referenceMyInterface, referenceChildClass2.BaseInterfaces[0]);
            Assert.Equal(referenceOtherInterface, referenceChildClass2.BaseInterfaces[1]);

            Assert.Equal("Project1.MyNamespace.Model", referenceModel.Name);
            Assert.Equal(referenceNamespaceServices, referenceModel.NamespaceReference);
            Assert.Equal("class", referenceModel.ClassType);
            Assert.Equal("public", referenceModel.AccessModifier);
            Assert.Equal("", referenceModel.Modifier);
            Assert.Equal(objectClassModel, referenceModel.BaseClass);
            Assert.Equal(1, referenceModel.BaseInterfaces.Count);
            Assert.Equal(referenceOtherInterface, referenceModel.BaseInterfaces[0]);

            Assert.Equal("Project1.MyNamespace.ChildClass3", referenceChildClass3.Name);
            Assert.Equal(referenceNamespaceServices, referenceChildClass3.NamespaceReference);
            Assert.Equal("class", referenceChildClass3.ClassType);
            Assert.Equal("public", referenceChildClass3.AccessModifier);
            Assert.Equal("", referenceChildClass3.Modifier);
            Assert.Equal(referenceChildClass1, referenceChildClass3.BaseClass);
            Assert.Empty(referenceChildClass3.BaseInterfaces);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesWithClassHierarchyAsParameter_UsingCSharpFactExtractor()
        {
            const string fileContent = @"
          namespace Project1.MyNamespace
          {
              public class BaseClass
              {
                  public int X;

                  public BaseClass() {}
              }

              public class ChildClass1 : BaseClass
              {
              }

              public class ChildClass2 : BaseClass
              {
                  public float Z;
              }

              public class Model
              {
              }

              public class ChildClass3 : ChildClass1
              {
                  private readonly Model _model;
              }

              public class Caller
              {
                  public void Call(BaseClass c)
                  {
                  }

                  public static void Call()
                  {
                      var caller = new Caller();
                      
                      caller.Call(new BaseClass());
                      caller.Call(new ChildClass1());
                      caller.Call(new ChildClass2());
                      caller.Call(new ChildClass3());

                      BaseClass a = new ChildClass1();
                      caller.Call(a);
                      a = new ChildClass3();
                      caller.Call(a);
                  }
              }
          }";


            var classModels = _extractor.Extract(fileContent).ClassTypes;

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
                                Name = "Project1.MyNamespace",
                                ClassModels = classModels.Cast<ClassModel>().ToList(),
                            }
                        }
                    }
                }
            };

            var referenceSolutionModel = _sut.Process(solutionModel);

            Assert.Equal(1, referenceSolutionModel.Projects.Count);
            var allCreatedReferences = referenceSolutionModel.CreatedClassModels;
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

            Assert.Equal("object", objectClassModel.Name);
            Assert.Equal("int", intClassModel.Name);
            Assert.Equal("float", floatClassModel.Name);
            Assert.Equal("void", voidClassModel.Name);

            var referenceNamespaceServices = referenceSolutionModel.Projects[0].Namespaces[0];
            var baseClass = referenceNamespaceServices.ClassModels[0];
            var childClass1 = referenceNamespaceServices.ClassModels[1];
            var childClass2 = referenceNamespaceServices.ClassModels[2];
            var modelClass = referenceNamespaceServices.ClassModels[3];
            var childClass3 = referenceNamespaceServices.ClassModels[4];
            var callerClass = referenceNamespaceServices.ClassModels[5];

            Assert.Equal(6, referenceNamespaceServices.ClassModels.Count);

            Assert.Equal("Project1.MyNamespace.BaseClass", baseClass.Name);
            Assert.Equal(referenceNamespaceServices, baseClass.NamespaceReference);
            Assert.Equal(1, baseClass.Constructors.Count);
            Assert.Equal("BaseClass", baseClass.Constructors[0].Name);
            Assert.Equal("", baseClass.Constructors[0].Modifier);
            Assert.Equal("public", baseClass.Constructors[0].AccessModifier);
            Assert.Empty(baseClass.Constructors[0].CalledMethods);
            Assert.Equal(baseClass, baseClass.Constructors[0].ContainingClass);
            Assert.Empty(baseClass.Constructors[0].Parameters);
            Assert.Null(baseClass.Constructors[0].ReturnTypeReferenceClassModel);
            Assert.Empty(baseClass.Methods);
            Assert.Empty(baseClass.Metrics);
            Assert.Equal(1, baseClass.Fields.Count);

            var baseClassFieldX = baseClass.Fields[0];
            Assert.Equal("X", baseClassFieldX.Name);
            Assert.Equal(baseClass, baseClassFieldX.ContainingClass);
            Assert.Equal(intClassModel, baseClassFieldX.Type);
            Assert.Equal("", baseClassFieldX.Modifier);
            Assert.Equal("public", baseClassFieldX.AccessModifier);
            Assert.False(baseClassFieldX.IsEvent);


            Assert.Equal("Project1.MyNamespace.ChildClass1", childClass1.Name);
            Assert.Equal(referenceNamespaceServices, childClass1.NamespaceReference);
            Assert.Empty(childClass1.Methods);
            Assert.Empty(childClass1.Metrics);
            Assert.Empty(childClass1.Fields);


            Assert.Equal("Project1.MyNamespace.ChildClass2", childClass2.Name);
            Assert.Equal(referenceNamespaceServices, childClass2.NamespaceReference);
            Assert.Empty(childClass2.Methods);
            Assert.Empty(childClass2.Metrics);
            Assert.Equal(1, childClass2.Fields.Count);

            var childClass2FieldZ = childClass2.Fields[0];
            Assert.Equal("Z", childClass2FieldZ.Name);
            Assert.Equal(childClass2, childClass2FieldZ.ContainingClass);
            Assert.Equal(floatClassModel, childClass2FieldZ.Type);
            Assert.Equal("", childClass2FieldZ.Modifier);
            Assert.Equal("public", childClass2FieldZ.AccessModifier);
            Assert.False(childClass2FieldZ.IsEvent);


            Assert.Equal("Project1.MyNamespace.Model", modelClass.Name);
            Assert.Equal(referenceNamespaceServices, modelClass.NamespaceReference);
            Assert.Empty(modelClass.Methods);
            Assert.Empty(modelClass.Metrics);
            Assert.Empty(modelClass.Fields);


            Assert.Equal("Project1.MyNamespace.ChildClass3", childClass3.Name);
            Assert.Equal(referenceNamespaceServices, childClass3.NamespaceReference);
            Assert.Empty(childClass3.Methods);
            Assert.Empty(childClass3.Metrics);
            Assert.Equal(1, childClass3.Fields.Count);

            var childClass3ModelField = childClass3.Fields[0];
            Assert.Equal("_model", childClass3ModelField.Name);
            Assert.Equal(childClass3, childClass3ModelField.ContainingClass);
            Assert.Equal(modelClass, childClass3ModelField.Type);
            Assert.Equal("readonly", childClass3ModelField.Modifier);
            Assert.Equal("private", childClass3ModelField.AccessModifier);
            Assert.False(childClass3ModelField.IsEvent);


            Assert.Equal("Project1.MyNamespace.Caller", callerClass.Name);
            Assert.Equal(referenceNamespaceServices, callerClass.NamespaceReference);
            Assert.Empty(callerClass.Fields);
            Assert.Empty(callerClass.Metrics);
            Assert.Equal(2, callerClass.Methods.Count);

            var callMethod0 = callerClass.Methods[0];
            Assert.Equal("Call", callMethod0.Name);
            Assert.Equal(callerClass, callMethod0.ContainingClass);
            Assert.Equal("", callMethod0.Modifier);
            Assert.Equal("public", callMethod0.AccessModifier);
            Assert.Equal(voidClassModel, callMethod0.ReturnTypeReferenceClassModel);
            Assert.Equal(1, callMethod0.Parameters.Count);
            Assert.Equal(baseClass, callMethod0.Parameters[0].Type);
            Assert.Equal("", callMethod0.Parameters[0].Modifier);
            Assert.Null(callMethod0.Parameters[0].DefaultValue);
            Assert.Empty(callMethod0.CalledMethods);
            Assert.False(callMethod0.IsConstructor);


            var callMethod1 = callerClass.Methods[1];
            Assert.Equal("Call", callMethod1.Name);
            Assert.Equal(callerClass, callMethod1.ContainingClass);
            Assert.Equal("static", callMethod1.Modifier);
            Assert.Equal("public", callMethod1.AccessModifier);
            Assert.Equal(voidClassModel, callMethod1.ReturnTypeReferenceClassModel);
            Assert.Empty(callMethod1.Parameters);
            Assert.Equal(6, callMethod1.CalledMethods.Count);
            Assert.False(callMethod1.IsConstructor);

            foreach (var calledMethod in callMethod1.CalledMethods)
            {
                Assert.Equal(callMethod0, calledMethod);
            }
        }
    }
}
