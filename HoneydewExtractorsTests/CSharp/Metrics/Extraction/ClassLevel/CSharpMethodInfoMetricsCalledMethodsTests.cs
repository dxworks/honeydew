using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoMetricsCalledMethodsTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpMethodInfoMetricsCalledMethodsTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpMethodInfoMetric>();
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorThatCallsOtherMethods()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                          public class Foo {
                                             
                                             public Foo(int a) {            
                                                 Function(Compute(a));                                
                                             }
     
                                             private void Function(int a) {}

                                             public int Compute(int a) {return 2*a;}  
                                         }                                        
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(1, classModels[0].Constructors.Count);
            Assert.Equal(2, classModels[0].Methods.Count);

            var functionMethod = classModels[0].Methods[0];
            Assert.Equal("Function", functionMethod.Name);
            Assert.Equal("void", functionMethod.ReturnType.Name);
            Assert.Equal(1, functionMethod.ParameterTypes.Count);
            var functionMethodParameter = (ParameterModel)functionMethod.ParameterTypes[0];
            Assert.Equal("int", functionMethodParameter.Name);
            Assert.Equal("", functionMethodParameter.Modifier);
            Assert.Null(functionMethodParameter.DefaultValue);
            Assert.Equal("TopLevel.Foo", functionMethod.ContainingTypeName);
            Assert.Equal("private", functionMethod.AccessModifier);
            Assert.Equal("", functionMethod.Modifier);
            Assert.Empty(functionMethod.CalledMethods);

            var computeMethod = classModels[0].Methods[1];
            Assert.Equal("Compute", computeMethod.Name);
            Assert.Equal("int", computeMethod.ReturnType.Name);
            Assert.Equal(1, computeMethod.ParameterTypes.Count);
            var computeMethodParameter = (ParameterModel)computeMethod.ParameterTypes[0];
            Assert.Equal("int", computeMethodParameter.Name);
            Assert.Equal("", computeMethodParameter.Modifier);
            Assert.Null(computeMethodParameter.DefaultValue);
            Assert.Equal("TopLevel.Foo", computeMethod.ContainingTypeName);
            Assert.Equal("public", computeMethod.AccessModifier);
            Assert.Equal("", computeMethod.Modifier);
            Assert.Empty(computeMethod.CalledMethods);

            var intArgConstructor = classModels[0].Constructors[0];
            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            var intArgConstructorParameter = (ParameterModel)intArgConstructor.ParameterTypes[0];
            Assert.Equal("int", intArgConstructorParameter.Name);
            Assert.Equal("", intArgConstructorParameter.Modifier);
            Assert.Null(intArgConstructorParameter.DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal("public", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Equal(2, intArgConstructor.CalledMethods.Count);

            Assert.Equal("Function", intArgConstructor.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);

            var methodSignatureType = intArgConstructor.CalledMethods[1];
            Assert.Equal("Compute", methodSignatureType.Name);
            Assert.Equal("TopLevel.Foo", methodSignatureType.ContainingTypeName);
            Assert.Equal(1, methodSignatureType.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)methodSignatureType.ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallEachOther()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                         {
                                             public Foo() : this(2) { }

                                             public Foo(int a) : this(""value"") { Print(); }

                                             public Foo(string a, int b = 2) { }
                                             
                                             public void Print(){}
                                       }                                    
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(1, classModels[0].Methods.Count);
            Assert.Equal(3, classModels[0].Constructors.Count);

            var noArgConstructor = classModels[0].Constructors[0];
            var intArgConstructor = classModels[0].Constructors[1];
            var stringIntArgConstructor = classModels[0].Constructors[2];
            var printMethod = classModels[0].Methods[0];

            Assert.Equal("Foo", noArgConstructor.Name);
            Assert.Empty(noArgConstructor.ParameterTypes);
            Assert.Equal("TopLevel.Foo", noArgConstructor.ContainingTypeName);
            Assert.Equal("public", noArgConstructor.AccessModifier);
            Assert.Equal("", noArgConstructor.Modifier);
            Assert.Equal(1, noArgConstructor.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructor.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", noArgConstructor.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, noArgConstructor.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)noArgConstructor.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);


            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)intArgConstructor.ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal("public", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Equal(2, intArgConstructor.CalledMethods.Count);

            Assert.Equal("Foo", intArgConstructor.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].ContainingTypeName);
            Assert.Equal(2, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("string", parameterModel3.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            var parameterModel4 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[1];
            Assert.Equal("int", parameterModel4.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Equal("2", parameterModel4.DefaultValue);

            Assert.Equal("Print", intArgConstructor.CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[1].ContainingTypeName);
            Assert.Empty(intArgConstructor.CalledMethods[1].ParameterTypes);


            Assert.Equal("Foo", stringIntArgConstructor.Name);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingTypeName);
            Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
            var parameterModel5 = (ParameterModel)stringIntArgConstructor.ParameterTypes[0];
            Assert.Equal("string", parameterModel5.Name);
            Assert.Equal("", parameterModel5.Modifier);
            Assert.Null(parameterModel5.DefaultValue);
            var parameterModel6 = (ParameterModel)stringIntArgConstructor.ParameterTypes[1];
            Assert.Equal("int", parameterModel6.Name);
            Assert.Equal("", parameterModel6.Modifier);
            Assert.Equal("2", parameterModel6.DefaultValue);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingTypeName);
            Assert.Equal("public", stringIntArgConstructor.AccessModifier);
            Assert.Equal("", stringIntArgConstructor.Modifier);
            Assert.Empty(stringIntArgConstructor.CalledMethods);

            Assert.Equal("Print", printMethod.Name);
            Assert.Equal("TopLevel.Foo", printMethod.ContainingTypeName);
            Assert.Equal("void", printMethod.ReturnType.Name);
            Assert.Empty(printMethod.ParameterTypes);
            Assert.Equal("public", printMethod.AccessModifier);
            Assert.Equal("", printMethod.Modifier);
            Assert.Empty(printMethod.CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallEachOtherAndBaseConstructor()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                         {
                                             public Foo() : this(2) { }

                                             public Foo(int a):this(a, 6) { }
                                             
                                             public Foo(int a, int b) { }
                                         }
                                         
                                         public class Bar : Foo
                                         {
                                             public Bar() : base(2) { }

                                             public Bar(int a) : base() {  }

                                             public Bar(string a,in int b=52) : this() { }
                                         }                                   
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Empty(classModels[0].Methods);
            Assert.Empty(classModels[1].Methods);

            Assert.Equal(3, classModels[0].Constructors.Count);
            Assert.Equal(3, classModels[1].Constructors.Count);

            var noArgConstructorBase = classModels[0].Constructors[0];
            var intArgConstructorBase = classModels[0].Constructors[1];
            var intIntConstructorBase = classModels[0].Constructors[2];

            AssertBasicConstructorInfo(noArgConstructorBase, "Foo");
            Assert.Empty(noArgConstructorBase.ParameterTypes);
            Assert.Equal(1, noArgConstructorBase.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorBase.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", noArgConstructorBase.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, noArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)noArgConstructorBase.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
            Assert.Equal(1, intArgConstructorBase.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)intArgConstructorBase.ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal(1, intArgConstructorBase.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorBase.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructorBase.CalledMethods[0].ContainingTypeName);
            Assert.Equal(2, intArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)intArgConstructorBase.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel3.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            var parameterModel4 = (ParameterModel)intArgConstructorBase.CalledMethods[0].ParameterTypes[1];
            Assert.Equal("int", parameterModel4.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Null(parameterModel4.DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
            Assert.Equal(2, intIntConstructorBase.ParameterTypes.Count);
            var parameterModel5 = (ParameterModel)intIntConstructorBase.ParameterTypes[0];
            Assert.Equal("int", parameterModel5.Name);
            Assert.Equal("", parameterModel5.Modifier);
            Assert.Null(parameterModel5.DefaultValue);
            var parameterModel6 = (ParameterModel)intIntConstructorBase.ParameterTypes[1];
            Assert.Equal("int", parameterModel6.Name);
            Assert.Equal("", parameterModel6.Modifier);
            Assert.Null(parameterModel6.DefaultValue);
            Assert.Empty(intIntConstructorBase.CalledMethods);

            var noArgConstructorChild = classModels[1].Constructors[0];
            var intArgConstructorChild = classModels[1].Constructors[1];
            var stringIntConstructorBase = classModels[1].Constructors[2];

            AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
            Assert.Empty(noArgConstructorChild.ParameterTypes);
            Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", noArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, noArgConstructorChild.CalledMethods[0].ParameterTypes.Count);
            var parameterModel7 = (ParameterModel)noArgConstructorChild.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel7.Name);
            Assert.Equal("", parameterModel7.Modifier);
            Assert.Null(parameterModel7.DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
            var parameterModel8 = (ParameterModel)intArgConstructorChild.ParameterTypes[0];
            Assert.Equal("int", parameterModel8.Name);
            Assert.Equal("", parameterModel8.Modifier);
            Assert.Null(parameterModel8.DefaultValue);
            Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(2, stringIntConstructorBase.ParameterTypes.Count);
            var parameterModel9 = (ParameterModel)stringIntConstructorBase.ParameterTypes[0];
            Assert.Equal("string", parameterModel9.Name);
            Assert.Equal("", parameterModel9.Modifier);
            Assert.Null(parameterModel9.DefaultValue);
            var parameterModel10 = (ParameterModel)stringIntConstructorBase.ParameterTypes[1];
            Assert.Equal("int", parameterModel10.Name);
            Assert.Equal("in", parameterModel10.Modifier);
            Assert.Equal("52", parameterModel10.DefaultValue);
            Assert.Equal(1, stringIntConstructorBase.CalledMethods.Count);
            Assert.Equal("Bar", stringIntConstructorBase.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Bar", stringIntConstructorBase.CalledMethods[0].ContainingTypeName);
            Assert.Empty(stringIntConstructorBase.CalledMethods[0].ParameterTypes);

            static void AssertBasicConstructorInfo(IConstructorType constructorModel, string className)
            {
                Assert.Equal(className, constructorModel.Name);
                Assert.Equal($"TopLevel.{className}", constructorModel.ContainingTypeName);
                Assert.Equal("public", constructorModel.AccessModifier);
                Assert.Equal("", constructorModel.Modifier);
            }
        }

        [Fact]
        public void
            Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallsBaseConstructor_ButBaseClassIsNotPresentInCompilationUnit()
        {
            const string fileContent = @"using System;

                                      namespace TopLevel
                                      {
                                         public class Bar : Foo
                                         {
                                             public Bar() : base(2) { }

                                             public Bar(int a) : base() {  }
                                         }                                   
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Empty(classModels[0].Methods);

            Assert.Equal(2, classModels[0].Constructors.Count);

            var noArgConstructorChild = classModels[0].Constructors[0];
            var intArgConstructorChild = classModels[0].Constructors[1];

            AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
            Assert.Empty(noArgConstructorChild.ParameterTypes);
            Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Empty(noArgConstructorChild.CalledMethods[0].ParameterTypes);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
            var parameterModel = (ParameterModel)intArgConstructorChild.ParameterTypes[0];
            Assert.Equal("int", parameterModel.Name);
            Assert.Equal("", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);
            Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

            static void AssertBasicConstructorInfo(IConstructorType constructorModel, string className)
            {
                Assert.Equal(className, constructorModel.Name);
                Assert.Equal($"TopLevel.{className}", constructorModel.ContainingTypeName);
                Assert.Equal("public", constructorModel.AccessModifier);
                Assert.Equal("", constructorModel.Modifier);
            }
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsMethodsFromAFieldOfADifferentClass()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                            public class Foo 
                                            {
                                                public void Method(int a) 
                                                {
                                                }
                                            }

                                             public class Bar
                                             {
                                                private Foo _foo;

                                                void M()
                                                {
                                                    _foo.Method(2);    
                                                }
                                             }                                  
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModel = classModels[1].Methods[0];

            Assert.Equal("M", methodModel.Name);
            Assert.Equal(1, methodModel.CalledMethods.Count);
            Assert.Equal("Method", methodModel.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", methodModel.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, methodModel.CalledMethods[0].ParameterTypes.Count);
            var parameterModel = (ParameterModel)methodModel.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);
            Assert.Equal("int", parameterModel.Name);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsStaticMethods()
        {
            const string fileContent = @"using System;

                                          namespace TopLevel
                                          {
                                                public class Foo 
                                                {
                                                    public static void Method(int a) 
                                                    {
                                                    }
                                                }
    
                                                 public class Bar
                                                 {
                                                    private static void OtherMethod(){}
    
                                                    void M()
                                                    {
                                                        OtherMethod();
                                                        Foo.Method(2);    
                                                        int.Parse(""5"");
                                                    }
                                                 }                                  
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[1].Methods[1];
            Assert.Equal("M", methodModelM.Name);
            Assert.Equal(3, methodModelM.CalledMethods.Count);

            var calledMethod1 = methodModelM.CalledMethods[0];
            Assert.Equal("OtherMethod", calledMethod1.Name);
            Assert.Equal("TopLevel.Bar", calledMethod1.ContainingTypeName);
            Assert.Empty(calledMethod1.ParameterTypes);

            var calledMethod2 = methodModelM.CalledMethods[1];
            Assert.Equal("Method", calledMethod2.Name);
            Assert.Equal("TopLevel.Foo", calledMethod2.ContainingTypeName);
            Assert.Equal(1, calledMethod2.ParameterTypes.Count);
            var calledMethod2Parameter = (ParameterModel)calledMethod2.ParameterTypes[0];
            Assert.Equal("", calledMethod2Parameter.Modifier);
            Assert.Null(calledMethod2Parameter.DefaultValue);
            Assert.Equal("int", calledMethod2Parameter.Name);

            var calledMethod3 = methodModelM.CalledMethods[2];
            Assert.Equal("Parse", calledMethod3.Name);
            Assert.Equal("int", calledMethod3.ContainingTypeName);
            Assert.Equal(1, calledMethod3.ParameterTypes.Count);
            var calledMethod3Parameter = (ParameterModel)calledMethod3.ParameterTypes[0];
            Assert.Equal("", calledMethod3Parameter.Modifier);
            Assert.Null(calledMethod3Parameter.DefaultValue);
            Assert.Equal("string", calledMethod3Parameter.Name);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsStaticMethodsFromUnknownClass()
        {
            const string fileContent = @"using System;
                                          
                                          namespace TopLevel
                                          {                                                   
                                                 public class Bar
                                                 {
                                                    void M()
                                                    {
                                                        Foo.Method(2);    
                                                    }
                                                 }                                  
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[0];
            Assert.Equal("M", methodModelM.Name);
            Assert.Equal(1, methodModelM.CalledMethods.Count);

            var calledMethod = methodModelM.CalledMethods[0];
            Assert.Equal("Method", calledMethod.Name);
            Assert.Equal("Foo", calledMethod.ContainingTypeName);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            var calledMethodParameter = (ParameterModel)calledMethod.ParameterTypes[0];
            Assert.Equal("", calledMethodParameter.Modifier);
            Assert.Equal("System.Int32", calledMethodParameter.Name);
            Assert.Null(calledMethodParameter.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsFuncLambdas()
        {
            const string fileContent = @"using System;

                                          namespace TopLevel
                                          {    
                                                 public class Bar
                                                 {
                                                    void Other(Func<int> a)
                                                    {
                                                    } 
                                                    void Method()
                                                    {
                                                        Func<int> func = () => 66;
                                                        Other(func);
                                                        Other(() => 0);
                                                        Other(() => { return 6;});
                                                        Other(A);
                                                        Other(delegate { return 7;});
                                                    }

                                                    private int A()
                                                    {
                                                        throw new NotImplementedException();
                                                    }
                                                 }                                  
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[1];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(5, methodModelM.CalledMethods.Count);

            foreach (var calledMethod in methodModelM.CalledMethods)
            {
                Assert.Equal("Other", calledMethod.Name);
                Assert.Equal("TopLevel.Bar", calledMethod.ContainingTypeName);
                Assert.Equal(1, calledMethod.ParameterTypes.Count);
                var calledMethodParameter = (ParameterModel)calledMethod.ParameterTypes[0];
                Assert.Equal("", calledMethodParameter.Modifier);
                Assert.Null(calledMethodParameter.DefaultValue);
                Assert.Equal("System.Func<int>", calledMethodParameter.Name);
            }
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsActionLambdas()
        {
            const string fileContent = @"using System;
                                          namespace TopLevel
                                          {    
                                                 
                                            public class Bar
                                            {
                                                void Other(System.Action<int> a)
                                                {
                                                }

                                                void Method()
                                                {
                                                    Other(i => { });
                                                                                            
                                                    Other(i =>
                                                    {
                                                        i++;
                                                    });
                                                                                            
                                                    Other(delegate(int i) {  });
                                                    Other(A);
                                                }

                                                private void A(int obj)
                                                {
                                                    throw new System.NotImplementedException();
                                                }
                                            }                                      
                                          }";
            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[1];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(4, methodModelM.CalledMethods.Count);

            foreach (var calledMethod in methodModelM.CalledMethods)
            {
                Assert.Equal("Other", calledMethod.Name);
                Assert.Equal("TopLevel.Bar", calledMethod.ContainingTypeName);
                Assert.Equal(1, calledMethod.ParameterTypes.Count);
                var calledMethodParameter = (ParameterModel)calledMethod.ParameterTypes[0];
                Assert.Equal("", calledMethodParameter.Modifier);
                Assert.Null(calledMethodParameter.DefaultValue);
                Assert.Equal("System.Action<int>", calledMethodParameter.Name);
            }
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallMethodsInsideLambdas()
        {
            const string fileContent = @"using System;
                                          namespace TopLevel
                                          {           
                                             public class Bar
                                            {
                                                void Other(System.Action<int> a)
                                                {
                                                }

                                                void Other(Func<int> a)
                                                {
                                                }
                                                void Method()
                                                {
                                                    Other(i => { Calc(i);});

                                                    Other(() => Calc(2));
                                                }

                                                private int Calc(int a)
                                                {
                                                    return a * 2;
                                                }
                                            }                                       
                                          }";
            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[2];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(4, methodModelM.CalledMethods.Count);


            var calledMethod1 = methodModelM.CalledMethods[0];
            Assert.Equal("Other", calledMethod1.Name);
            Assert.Equal("TopLevel.Bar", calledMethod1.ContainingTypeName);
            Assert.Equal(1, calledMethod1.ParameterTypes.Count);
            var calledMethod1Parameter = (ParameterModel)calledMethod1.ParameterTypes[0];
            Assert.Equal("", calledMethod1Parameter.Modifier);
            Assert.Null(calledMethod1Parameter.DefaultValue);
            Assert.Equal("System.Action<int>", calledMethod1Parameter.Name);

            var calledMethod2 = methodModelM.CalledMethods[1];
            Assert.Equal("Calc", calledMethod2.Name);
            Assert.Equal("TopLevel.Bar", calledMethod2.ContainingTypeName);
            Assert.Equal(1, calledMethod2.ParameterTypes.Count);
            var calledMethod2Parameter = (ParameterModel)calledMethod2.ParameterTypes[0];
            Assert.Equal("", calledMethod2Parameter.Modifier);
            Assert.Null(calledMethod2Parameter.DefaultValue);
            Assert.Equal("int", calledMethod2Parameter.Name);

            var calledMethod3 = methodModelM.CalledMethods[2];
            Assert.Equal("Other", calledMethod3.Name);
            Assert.Equal("TopLevel.Bar", calledMethod3.ContainingTypeName);
            Assert.Equal(1, calledMethod3.ParameterTypes.Count);
            var calledMethod3Parameter = (ParameterModel)calledMethod3.ParameterTypes[0];
            Assert.Equal("", calledMethod3Parameter.Modifier);
            Assert.Null(calledMethod3Parameter.DefaultValue);
            Assert.Equal("System.Func<int>", calledMethod3Parameter.Name);

            var calledMethod4 = methodModelM.CalledMethods[3];
            Assert.Equal("Calc", calledMethod4.Name);
            Assert.Equal("TopLevel.Bar", calledMethod4.ContainingTypeName);
            Assert.Equal(1, calledMethod4.ParameterTypes.Count);
            var calledMethod4Parameter = (ParameterModel)calledMethod4.ParameterTypes[0];
            Assert.Equal("", calledMethod4Parameter.Modifier);
            Assert.Null(calledMethod4Parameter.DefaultValue);
            Assert.Equal("int", calledMethod4Parameter.Name);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallHasCalledMethodsChained()
        {
            const string fileContent = @"using System;

                                          namespace TopLevel
                                          {           
                                            class Foo
                                            {
                                                public string GetName()
                                                {
                                                    return """";
                                                } 
                                            }
                                            
                                            class Builder
                                            {
                                                public Builder Set()
                                                {
                                                    return this;
                                                }

                                                public Foo Build()
                                                {
                                                    return new Foo();
                                                }
                                            }
                                            
                                            public class Bar
                                            {
                                                
                                                void Method()
                                                {
                                                    var a = Create()
                                                        .Set()
                                                        .Build()
                                                        .GetName().Trim();
                                                }

                                                private Builder Create()
                                                {
                                                    return new Builder();
                                                }
                                            }                                          
                                          }";
            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[2].Methods[0];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(5, methodModelM.CalledMethods.Count);

            var calledMethod0 = methodModelM.CalledMethods[0];
            Assert.Equal("Trim", calledMethod0.Name);
            Assert.Equal("string", calledMethod0.ContainingTypeName);
            Assert.Empty(calledMethod0.ParameterTypes);

            var calledMethod1 = methodModelM.CalledMethods[1];
            Assert.Equal("GetName", calledMethod1.Name);
            Assert.Equal("TopLevel.Foo", calledMethod1.ContainingTypeName);
            Assert.Empty(calledMethod1.ParameterTypes);

            var calledMethod2 = methodModelM.CalledMethods[2];
            Assert.Equal("Build", calledMethod2.Name);
            Assert.Equal("TopLevel.Builder", calledMethod2.ContainingTypeName);
            Assert.Empty(calledMethod2.ParameterTypes);

            var calledMethod3 = methodModelM.CalledMethods[3];
            Assert.Equal("Set", calledMethod3.Name);
            Assert.Equal("TopLevel.Builder", calledMethod3.ContainingTypeName);
            Assert.Empty(calledMethod3.ParameterTypes);

            var calledMethod4 = methodModelM.CalledMethods[4];
            Assert.Equal("Create", calledMethod4.Name);
            Assert.Equal("TopLevel.Bar", calledMethod4.ContainingTypeName);
            Assert.Empty(calledMethod4.ParameterTypes);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsLinqMethods()
        {
            const string fileContent = @"
                                        using System;
                                        using System.Collections.Generic;
                                        using System.Linq;
                                          namespace TopLevel
                                          {                                                       
                                            public class Bar
                                            {
                                                void Method()
                                                {
                                                    var list = new List<string>();
                                                    var enumerable = list.Where(s => s != null).Skip(6).Select(s=>s.Trim()).ToList();
                                                }
                                            }                                          
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[0];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(5, methodModelM.CalledMethods.Count);

            var calledMethod0 = methodModelM.CalledMethods[0];
            Assert.Equal("ToList", calledMethod0.Name);
            Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod0.ContainingTypeName);
            Assert.Empty(calledMethod0.ParameterTypes);

            var calledMethod1 = methodModelM.CalledMethods[1];
            Assert.Equal("Select", calledMethod1.Name);
            Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod1.ContainingTypeName);
            Assert.Equal(1, calledMethod1.ParameterTypes.Count);
            var calledMethod1Parameter = (ParameterModel)calledMethod1.ParameterTypes[0];
            Assert.Equal("", calledMethod1Parameter.Modifier);
            Assert.Equal("System.Func<string, string>", calledMethod1Parameter.Name);
            Assert.Null(calledMethod1Parameter.DefaultValue);

            var calledMethod2 = methodModelM.CalledMethods[2];
            Assert.Equal("Skip", calledMethod2.Name);
            Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod2.ContainingTypeName);
            Assert.Equal(1, calledMethod2.ParameterTypes.Count);
            var calledMethod2Parameter = (ParameterModel)calledMethod2.ParameterTypes[0];
            Assert.Equal("", calledMethod2Parameter.Modifier);
            Assert.Equal("int", calledMethod2Parameter.Name);
            Assert.Null(calledMethod2Parameter.DefaultValue);

            var calledMethod3 = methodModelM.CalledMethods[3];
            Assert.Equal("Where", calledMethod3.Name);
            Assert.Equal("System.Collections.Generic.List<string>", calledMethod3.ContainingTypeName);
            Assert.Equal(1, calledMethod3.ParameterTypes.Count);
            var calledMethod3Parameter = (ParameterModel)calledMethod3.ParameterTypes[0];
            Assert.Equal("", calledMethod3Parameter.Modifier);
            Assert.Equal("System.Func<string, bool>", calledMethod3Parameter.Name);
            Assert.Null(calledMethod3Parameter.DefaultValue);

            var calledMethod4 = methodModelM.CalledMethods[4];
            Assert.Equal("Trim", calledMethod4.Name);
            Assert.Equal("string", calledMethod4.ContainingTypeName);
            Assert.Empty(calledMethod4.ParameterTypes);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsMethodsFromDictionaryOfACastedObject()
        {
            const string fileContent = @"
                                        using System;
                                        using System.Collections.Generic;                                        
                                          namespace TopLevel
                                          {                                                       
                                            class Foo
                                            {
                                                public Dictionary<string, string> MyDictionary = new();
                                            }
                                            
                                            public class Bar
                                            {
                                                void Method()
                                                {
                                                   object foo = new Foo();
                                                    if (((Foo)foo).MyDictionary.TryGetValue(""value"", out var value))
                                                    {                                                        
                                                    }
                                                }
                                            }                                              
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[1].Methods[0];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(1, methodModelM.CalledMethods.Count);

            var calledMethod0 = methodModelM.CalledMethods[0];
            Assert.Equal("TryGetValue", calledMethod0.Name);
            Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.ContainingTypeName);

            Assert.Equal(2, calledMethod0.ParameterTypes.Count);

            var calledMethod0Parameter = (ParameterModel)calledMethod0.ParameterTypes[0];
            Assert.Equal("", calledMethod0Parameter.Modifier);
            Assert.Equal("string", calledMethod0Parameter.Name);
            Assert.Null(calledMethod0Parameter.DefaultValue);

            var method0Parameter = (ParameterModel)calledMethod0.ParameterTypes[1];
            Assert.Equal("out", method0Parameter.Modifier);
            Assert.Equal("string", method0Parameter.Name);
            Assert.Null(method0Parameter.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsMethodsFromAnotherClassAsProperty()
        {
            const string fileContent = @"
                                        using System;
                                        using System.Collections.Generic;                                        
                                          namespace TopLevel
                                          {                                                       
                                            class Foo
                                            {
                                                public Dictionary<string, string> MyDictionary = new();
                                            }
                                            
                                            public class Bar
                                            {
                                                Foo foo {get;set;}
                                                void Method()
                                                {                                                   
                                                    if (foo.MyDictionary.TryGetValue(""value"", out var value))
                                                    {                                                        
                                                    }
                                                }
                                            }                                              
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[1].Methods[0];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(1, methodModelM.CalledMethods.Count);

            var calledMethod0 = methodModelM.CalledMethods[0];
            Assert.Equal("TryGetValue", calledMethod0.Name);
            Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.ContainingTypeName);

            Assert.Equal(2, calledMethod0.ParameterTypes.Count);

            var calledMethod0Parameter = (ParameterModel)calledMethod0.ParameterTypes[0];
            Assert.Equal("", calledMethod0Parameter.Modifier);
            Assert.Equal("string", calledMethod0Parameter.Name);
            Assert.Null(calledMethod0Parameter.DefaultValue);

            var method0Parameter = (ParameterModel)calledMethod0.ParameterTypes[1];
            Assert.Equal("out", method0Parameter.Modifier);
            Assert.Equal("string", method0Parameter.Name);
            Assert.Null(method0Parameter.DefaultValue);
        }
    }
}
