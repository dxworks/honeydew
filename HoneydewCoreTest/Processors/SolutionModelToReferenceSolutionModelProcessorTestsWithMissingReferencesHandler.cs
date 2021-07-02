using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using HoneydewCore.Processors;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class SolutionModelToReferenceSolutionModelProcessorTestsWithMissingReferencesHandler
    {
        private readonly SolutionModelToReferenceSolutionModelProcessor _sut;

        public SolutionModelToReferenceSolutionModelProcessorTestsWithMissingReferencesHandler()
        {
            _sut = new SolutionModelToReferenceSolutionModelProcessor();
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithPrimitiveTypesAsParameters_UsingCSharpClassFactExtractor()
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

            var intClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "int");
            var stringClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "string");
            var floatClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "float");
            var voidClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "void");

            Assert.NotNull(intClassModel);
            Assert.NotNull(stringClassModel);
            Assert.NotNull(floatClassModel);
            Assert.NotNull(voidClassModel);

            Assert.Equal("int", intClassModel.Name);
            Assert.Equal("string", stringClassModel.Name);
            Assert.Equal("float", floatClassModel.Name);
            Assert.Equal("void", voidClassModel.Name);


            Assert.Equal(2, intClassModel.Methods.Count);

            var toStringMethodModel = intClassModel.Methods.First(model => model.Name == "ToString");
            var parseMethodModel = intClassModel.Methods.First(model => model.Name == "Parse");

            Assert.Equal("ToString", toStringMethodModel.Name);
            Assert.Equal(intClassModel, toStringMethodModel.ContainingClass);
            Assert.Empty(toStringMethodModel.ParameterTypes);

            Assert.Equal("Parse", parseMethodModel.Name);
            Assert.Equal(intClassModel, parseMethodModel.ContainingClass);
            Assert.Equal(1, parseMethodModel.ParameterTypes.Count);
            Assert.Equal(stringClassModel, parseMethodModel.ParameterTypes[0]);

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
            Assert.Equal(parseMethodModel, methodFunction2.CalledMethods[0]);

            Assert.Equal("Function3", methodFunction3.Name);
            Assert.Equal(referenceMyClass, methodFunction3.ContainingClass);
            Assert.Equal("", methodFunction3.Modifier);
            Assert.Equal("public", methodFunction3.AccessModifier);
            Assert.Equal(stringClassModel, methodFunction3.ReturnTypeReferenceClassModel);
            Assert.Equal(1, methodFunction3.ParameterTypes.Count);
            Assert.Equal(intClassModel, methodFunction3.ParameterTypes[0]);
            Assert.Equal(1, methodFunction3.CalledMethods.Count);
            Assert.Equal(toStringMethodModel, methodFunction3.CalledMethods[0]);

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
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesOnlyWithNumericesAsParameters_UsingCSharpClassFactExtractor()
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
            Assert.Equal(5, referenceSolutionModel.ClassModelsNotDeclaredInSolution.Count);

            var intClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "int");
            var shortClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "short");
            var longClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "long");
            var byteClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "byte");
            var voidClassModel =
                referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "void");

            Assert.NotNull(intClassModel);
            Assert.NotNull(shortClassModel);
            Assert.NotNull(longClassModel);
            Assert.NotNull(byteClassModel);
            Assert.NotNull(voidClassModel);

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
            Assert.Empty(printNoArg.ParameterTypes);
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
            Assert.Equal(1, printInt.ParameterTypes.Count);
            Assert.Equal(intClassModel, printInt.ParameterTypes[0]);
            Assert.Empty(printInt.CalledMethods);

            Assert.Equal("Print", printShort.Name);
            Assert.Equal(referenceMyClass, printShort.ContainingClass);
            Assert.Equal("", printShort.Modifier);
            Assert.Equal("private", printShort.AccessModifier);
            Assert.Equal(voidClassModel, printShort.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printShort.ParameterTypes.Count);
            Assert.Equal(shortClassModel, printShort.ParameterTypes[0]);
            Assert.Empty(printShort.CalledMethods);

            Assert.Equal("Print", printLong.Name);
            Assert.Equal(referenceMyClass, printLong.ContainingClass);
            Assert.Equal("", printLong.Modifier);
            Assert.Equal("private", printLong.AccessModifier);
            Assert.Equal(voidClassModel, printLong.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printLong.ParameterTypes.Count);
            Assert.Equal(longClassModel, printLong.ParameterTypes[0]);
            Assert.Empty(printLong.CalledMethods);

            Assert.Equal("Print", printByte.Name);
            Assert.Equal(referenceMyClass, printByte.ContainingClass);
            Assert.Equal("", printByte.Modifier);
            Assert.Equal("private", printByte.AccessModifier);
            Assert.Equal(voidClassModel, printByte.ReturnTypeReferenceClassModel);
            Assert.Equal(1, printByte.ParameterTypes.Count);
            Assert.Equal(byteClassModel, printByte.ParameterTypes[0]);
            Assert.Empty(printByte.CalledMethods);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnReferenceSolutionModelWithAllMethodReferences_WhenGivenASolutionModelWithClassesWithMethodReferencesWithClassHierarchyAsParameter_UsingCSharpClassFactExtractor()
        {
//             const string fileContent = @"
//         namespace Project1.MyNamespace
//         {
//             public class BaseClass
//             {
//                 public int X;
//             }
//
//             public class ChildClass1 : BaseClass
//             {
//             }
//
//             public class ChildClass2 : BaseClass
//             {
//                 public float Z;
//             }
//
//             public class Model
//             {
//             }
//
//             public class ChildClass3 : ChildClass1
//             {
//                 private readonly Model _model;
//             }
//
//             public class Caller
//             {
//                 public void Call(BaseClass c)
//                 {
//                 }
//
//                 public static void Call()
//                 {
//                     var caller = new Caller();
//                     
//                     caller.Call(new BaseClass());
//                     caller.Call(new ChildClass1());
//                     caller.Call(new ChildClass2());
//                     caller.Call(new ChildClass3());
//
//                     BaseClass a = new ChildClass1();
//                     caller.Call(a);
//                     a = new ChildClass3();
//                     caller.Call(a);
//                 }
//             }
//         }";
//
//             var extractor = new CSharpClassFactExtractor();
//             var classModels = extractor.Extract(fileContent);
//
//             var solutionModel = new SolutionModel
//             {
//                 Projects =
//                 {
//                     new ProjectModel
//                     {
//                         Name = "Project1",
//                         Namespaces =
//                         {
//                             {
//                                 "Project1.MyNamespace", new NamespaceModel
//                                 {
//                                     Name = "Project1.MyNamespace",
//                                     ClassModels = classModels,
//                                 }
//                             }
//                         }
//                     }
//                 }
//             };
//
//
//             var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));
//
//             var referenceSolutionModel = processable.Value;
//
//             Assert.Equal(1, referenceSolutionModel.Projects.Count);
//             Assert.Equal(5, referenceSolutionModel.ClassModelsNotDeclaredInSolution.Count);
//
//             var intClassModel =
//                 referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "int");
//             var floatClassModel =
//                 referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "float");
//
//             var voidClassModel =
//                 referenceSolutionModel.ClassModelsNotDeclaredInSolution.SingleOrDefault(a => a.Name == "void");
//
//             Assert.NotNull(intClassModel);
//             Assert.NotNull(floatClassModel);
//             Assert.NotNull(voidClassModel);
//
//             Assert.Equal("int", intClassModel.Name);
//             Assert.Equal("float", floatClassModel.Name);
//             Assert.Equal("void", voidClassModel.Name);
//
//             var referenceNamespaceServices = referenceSolutionModel.Projects[0].Namespaces[0];
//             var baseClass = referenceNamespaceServices.ClassModels[0];
//             var childClass1 = referenceNamespaceServices.ClassModels[1];
//             var childClass2 = referenceNamespaceServices.ClassModels[2];
//             var modelClass = referenceNamespaceServices.ClassModels[3];
//             var childClass3 = referenceNamespaceServices.ClassModels[4];
//             var callerClass = referenceNamespaceServices.ClassModels[5];
//
//             Assert.Equal(6, referenceNamespaceServices.ClassModels.Count);
//
//             Assert.Equal("Project1.MyNamespace.BaseClass", baseClass.Name);
//             Assert.Equal(referenceNamespaceServices, baseClass.NamespaceReference);
//             Assert.Empty(baseClass.Methods);
//             Assert.Empty(baseClass.Metrics);
//             Assert.Equal(1, baseClass.Fields.Count);
//
//             var baseClassFieldX = baseClass.Fields[0];
//             Assert.Equal("X", baseClassFieldX.Name);
//             Assert.Equal(baseClass, baseClassFieldX.ContainingClass);
//             Assert.Equal(intClassModel, baseClassFieldX.Type);
//             Assert.Equal("", baseClassFieldX.Modifier);
//             Assert.Equal("public", baseClassFieldX.AccessModifier);
//             Assert.False(baseClassFieldX.Inherited);
//             Assert.False(baseClassFieldX.IsEvent);
//
//             Assert.Equal("Project1.MyNamespace.ChildClass1", baseClass.Name);
//             Assert.Equal(referenceNamespaceServices, baseClass.NamespaceReference);
//             Assert.Empty(baseClass.Methods);
//             Assert.Empty(baseClass.Metrics);
//             Assert.Equal(1, baseClass.Fields.Count);
//
//             var baseClassFieldX = baseClass.Fields[0];
//             Assert.Equal("X", baseClassFieldX.Name);
//             Assert.Equal(baseClass, baseClassFieldX.ContainingClass);
//             Assert.Equal(intClassModel, baseClassFieldX.Type);
//             Assert.Equal("", baseClassFieldX.Modifier);
//             Assert.Equal("public", baseClassFieldX.AccessModifier);
//             Assert.False(baseClassFieldX.Inherited);
//             Assert.False(baseClassFieldX.IsEvent);
        }
    }
}