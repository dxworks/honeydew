﻿using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodCalls;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel;

public class CSharpCalledMethodsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpCalledMethodsTests()
    {
        var compositeVisitor = new CompositeVisitor();

        var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodCallVisitor>
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
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);
        Assert.Equal(2, classModel.Methods.Count);

        var functionMethod = classModel.Methods[0];
        Assert.Equal("Function", functionMethod.Name);
        Assert.Equal("void", functionMethod.ReturnValue.Type.Name);
        Assert.Equal(1, functionMethod.ParameterTypes.Count);
        var functionMethodParameter = (ParameterModel)functionMethod.ParameterTypes[0];
        Assert.Equal("int", functionMethodParameter.Type.Name);
        Assert.Equal("", functionMethodParameter.Modifier);
        Assert.Null(functionMethodParameter.DefaultValue);
        Assert.Equal("private", functionMethod.AccessModifier);
        Assert.Equal("", functionMethod.Modifier);
        Assert.Empty(functionMethod.CalledMethods);

        var computeMethod = classModel.Methods[1];
        Assert.Equal("Compute", computeMethod.Name);
        Assert.Equal("int", computeMethod.ReturnValue.Type.Name);
        Assert.Equal(1, computeMethod.ParameterTypes.Count);
        var computeMethodParameter = (ParameterModel)computeMethod.ParameterTypes[0];
        Assert.Equal("int", computeMethodParameter.Type.Name);
        Assert.Equal("", computeMethodParameter.Modifier);
        Assert.Null(computeMethodParameter.DefaultValue);
        Assert.Equal("public", computeMethod.AccessModifier);
        Assert.Equal("", computeMethod.Modifier);
        Assert.Empty(computeMethod.CalledMethods);

        var intArgConstructor = classModel.Constructors[0];
        Assert.Equal("Foo", intArgConstructor.Name);
        Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
        var intArgConstructorParameter = (ParameterModel)intArgConstructor.ParameterTypes[0];
        Assert.Equal("int", intArgConstructorParameter.Type.Name);
        Assert.Equal("", intArgConstructorParameter.Modifier);
        Assert.Null(intArgConstructorParameter.DefaultValue);
        Assert.Equal("public", intArgConstructor.AccessModifier);
        Assert.Equal("", intArgConstructor.Modifier);
        Assert.Equal(2, intArgConstructor.CalledMethods.Count);

        Assert.Equal("Function", intArgConstructor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].LocationClassName);
        Assert.Empty(intArgConstructor.CalledMethods[0].DefinitionMethodNames);
        Assert.Equal(1, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);

        var methodSignatureType = intArgConstructor.CalledMethods[1];
        Assert.Equal("Compute", methodSignatureType.Name);
        Assert.Equal("TopLevel.Foo", methodSignatureType.DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodSignatureType.LocationClassName);
        Assert.Empty(methodSignatureType.DefinitionMethodNames);
        Assert.Equal(1, methodSignatureType.ParameterTypes.Count);
        var parameterModel2 = (ParameterModel)methodSignatureType.ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModel = ((ClassModel)classTypes[1]).Methods[0];

        Assert.Equal("M", methodModel.Name);
        Assert.Equal(1, methodModel.CalledMethods.Count);
        Assert.Equal("Method", methodModel.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", methodModel.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodModel.CalledMethods[0].LocationClassName);
        Assert.Empty(methodModel.CalledMethods[0].DefinitionMethodNames);
        Assert.Equal(1, methodModel.CalledMethods[0].ParameterTypes.Count);
        var parameterModel = (ParameterModel)methodModel.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel.Modifier);
        Assert.Null(parameterModel.DefaultValue);
        Assert.Equal("int", parameterModel.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[1]).Methods[1];
        Assert.Equal("M", methodModelM.Name);
        Assert.Equal(3, methodModelM.CalledMethods.Count);

        var calledMethod1 = methodModelM.CalledMethods[0];
        Assert.Equal("OtherMethod", calledMethod1.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Empty(calledMethod1.ParameterTypes);

        var calledMethod2 = methodModelM.CalledMethods[1];
        Assert.Equal("Method", calledMethod2.Name);
        Assert.Equal("TopLevel.Foo", calledMethod2.DefinitionClassName);
        Assert.Equal("TopLevel.Foo", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.DefinitionMethodNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        var calledMethod2Parameter = (ParameterModel)calledMethod2.ParameterTypes[0];
        Assert.Equal("", calledMethod2Parameter.Modifier);
        Assert.Null(calledMethod2Parameter.DefaultValue);
        Assert.Equal("int", calledMethod2Parameter.Type.Name);

        var calledMethod3 = methodModelM.CalledMethods[2];
        Assert.Equal("Parse", calledMethod3.Name);
        Assert.Equal("int", calledMethod3.DefinitionClassName);
        Assert.Equal("int", calledMethod3.LocationClassName);
        Assert.Empty(calledMethod3.DefinitionMethodNames);
        Assert.Equal(1, calledMethod3.ParameterTypes.Count);
        var calledMethod3Parameter = (ParameterModel)calledMethod3.ParameterTypes[0];
        Assert.Equal("", calledMethod3Parameter.Modifier);
        Assert.Null(calledMethod3Parameter.DefaultValue);
        Assert.Equal("string", calledMethod3Parameter.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[0]).Methods[0];
        Assert.Equal("M", methodModelM.Name);
        Assert.Equal(1, methodModelM.CalledMethods.Count);

        var calledMethod = methodModelM.CalledMethods[0];
        Assert.Equal("Method", calledMethod.Name);
        Assert.Equal("Foo", calledMethod.DefinitionClassName);
        Assert.Equal("Foo", calledMethod.LocationClassName);
        Assert.Empty(calledMethod.DefinitionMethodNames);
        Assert.Equal(1, calledMethod.ParameterTypes.Count);
        var calledMethodParameter = (ParameterModel)calledMethod.ParameterTypes[0];
        Assert.Equal("", calledMethodParameter.Modifier);
        Assert.Equal("System.Int32", calledMethodParameter.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[0]).Methods[1];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(5, methodModelM.CalledMethods.Count);

        foreach (var calledMethod in methodModelM.CalledMethods)
        {
            Assert.Equal("Other", calledMethod.Name);
            Assert.Equal("TopLevel.Bar", calledMethod.DefinitionClassName);
            Assert.Equal("TopLevel.Bar", calledMethod.LocationClassName);
            Assert.Empty(calledMethod.DefinitionMethodNames);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            var calledMethodParameter = (ParameterModel)calledMethod.ParameterTypes[0];
            Assert.Equal("", calledMethodParameter.Modifier);
            Assert.Null(calledMethodParameter.DefaultValue);
            Assert.Equal("System.Func<int>", calledMethodParameter.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[0]).Methods[1];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(4, methodModelM.CalledMethods.Count);

        foreach (var calledMethod in methodModelM.CalledMethods)
        {
            Assert.Equal("Other", calledMethod.Name);
            Assert.Equal("TopLevel.Bar", calledMethod.DefinitionClassName);
            Assert.Equal("TopLevel.Bar", calledMethod.LocationClassName);
            Assert.Empty(calledMethod.DefinitionMethodNames);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            var calledMethodParameter = (ParameterModel)calledMethod.ParameterTypes[0];
            Assert.Equal("", calledMethodParameter.Modifier);
            Assert.Null(calledMethodParameter.DefaultValue);
            Assert.Equal("System.Action<int>", calledMethodParameter.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[0]).Methods[2];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(4, methodModelM.CalledMethods.Count);


        var calledMethod1 = methodModelM.CalledMethods[0];
        Assert.Equal("Other", calledMethod1.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Equal(1, calledMethod1.ParameterTypes.Count);
        var calledMethod1Parameter = (ParameterModel)calledMethod1.ParameterTypes[0];
        Assert.Equal("", calledMethod1Parameter.Modifier);
        Assert.Null(calledMethod1Parameter.DefaultValue);
        Assert.Equal("System.Action<int>", calledMethod1Parameter.Type.Name);

        var calledMethod2 = methodModelM.CalledMethods[1];
        Assert.Equal("Calc", calledMethod2.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        var calledMethod2Parameter = (ParameterModel)calledMethod2.ParameterTypes[0];
        Assert.Equal("", calledMethod2Parameter.Modifier);
        Assert.Null(calledMethod2Parameter.DefaultValue);
        Assert.Equal("int", calledMethod2Parameter.Type.Name);

        var calledMethod3 = methodModelM.CalledMethods[2];
        Assert.Equal("Other", calledMethod3.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Equal(1, calledMethod3.ParameterTypes.Count);
        var calledMethod3Parameter = (ParameterModel)calledMethod3.ParameterTypes[0];
        Assert.Equal("", calledMethod3Parameter.Modifier);
        Assert.Null(calledMethod3Parameter.DefaultValue);
        Assert.Equal("System.Func<int>", calledMethod3Parameter.Type.Name);

        var calledMethod4 = methodModelM.CalledMethods[3];
        Assert.Equal("Calc", calledMethod4.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Equal(1, calledMethod4.ParameterTypes.Count);
        var calledMethod4Parameter = (ParameterModel)calledMethod4.ParameterTypes[0];
        Assert.Equal("", calledMethod4Parameter.Modifier);
        Assert.Null(calledMethod4Parameter.DefaultValue);
        Assert.Equal("int", calledMethod4Parameter.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[2]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(5, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("Trim", calledMethod0.Name);
        Assert.Equal("string", calledMethod0.DefinitionClassName);
        Assert.Equal("string", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.DefinitionMethodNames);
        Assert.Empty(calledMethod0.ParameterTypes);

        var calledMethod1 = methodModelM.CalledMethods[1];
        Assert.Equal("GetName", calledMethod1.Name);
        Assert.Equal("TopLevel.Foo", calledMethod1.LocationClassName);
        Assert.Equal("TopLevel.Foo", calledMethod1.DefinitionClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Empty(calledMethod1.ParameterTypes);

        var calledMethod2 = methodModelM.CalledMethods[2];
        Assert.Equal("Build", calledMethod2.Name);
        Assert.Equal("TopLevel.Builder", calledMethod2.DefinitionClassName);
        Assert.Equal("TopLevel.Builder", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.DefinitionMethodNames);
        Assert.Empty(calledMethod2.ParameterTypes);

        var calledMethod3 = methodModelM.CalledMethods[3];
        Assert.Equal("Set", calledMethod3.Name);
        Assert.Equal("TopLevel.Builder", calledMethod3.DefinitionClassName);
        Assert.Equal("TopLevel.Builder", calledMethod3.LocationClassName);
        Assert.Empty(calledMethod3.DefinitionMethodNames);
        Assert.Empty(calledMethod3.ParameterTypes);

        var calledMethod4 = methodModelM.CalledMethods[4];
        Assert.Equal("Create", calledMethod4.Name);
        Assert.Equal("TopLevel.Bar", calledMethod4.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod4.LocationClassName);
        Assert.Empty(calledMethod4.DefinitionMethodNames);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[0]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(5, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("ToList", calledMethod0.Name);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod0.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.DefinitionMethodNames);
        Assert.Empty(calledMethod0.ParameterTypes);

        var calledMethod1 = methodModelM.CalledMethods[1];
        Assert.Equal("Select", calledMethod1.Name);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod1.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Equal(1, calledMethod1.ParameterTypes.Count);
        var calledMethod1Parameter = (ParameterModel)calledMethod1.ParameterTypes[0];
        Assert.Equal("", calledMethod1Parameter.Modifier);
        Assert.Equal("System.Func<string, string>", calledMethod1Parameter.Type.Name);
        Assert.Null(calledMethod1Parameter.DefaultValue);

        var calledMethod2 = methodModelM.CalledMethods[2];
        Assert.Equal("Skip", calledMethod2.Name);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod2.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.DefinitionMethodNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        var calledMethod2Parameter = (ParameterModel)calledMethod2.ParameterTypes[0];
        Assert.Equal("", calledMethod2Parameter.Modifier);
        Assert.Equal("int", calledMethod2Parameter.Type.Name);
        Assert.Null(calledMethod2Parameter.DefaultValue);

        var calledMethod3 = methodModelM.CalledMethods[3];
        Assert.Equal("Where", calledMethod3.Name);
        Assert.Equal("System.Collections.Generic.List<string>", calledMethod3.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.List<string>", calledMethod3.LocationClassName);
        Assert.Empty(calledMethod3.DefinitionMethodNames);
        Assert.Equal(1, calledMethod3.ParameterTypes.Count);
        var calledMethod3Parameter = (ParameterModel)calledMethod3.ParameterTypes[0];
        Assert.Equal("", calledMethod3Parameter.Modifier);
        Assert.Equal("System.Func<string, bool>", calledMethod3Parameter.Type.Name);
        Assert.Null(calledMethod3Parameter.DefaultValue);

        var calledMethod4 = methodModelM.CalledMethods[4];
        Assert.Equal("Trim", calledMethod4.Name);
        Assert.Equal("string", calledMethod4.DefinitionClassName);
        Assert.Equal("string", calledMethod4.LocationClassName);
        Assert.Empty(calledMethod4.DefinitionMethodNames);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[1]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(1, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("TryGetValue", calledMethod0.Name);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.DefinitionMethodNames);

        Assert.Equal(2, calledMethod0.ParameterTypes.Count);

        var calledMethod0Parameter = (ParameterModel)calledMethod0.ParameterTypes[0];
        Assert.Equal("", calledMethod0Parameter.Modifier);
        Assert.Equal("string", calledMethod0Parameter.Type.Name);
        Assert.Null(calledMethod0Parameter.DefaultValue);

        var method0Parameter = (ParameterModel)calledMethod0.ParameterTypes[1];
        Assert.Equal("out", method0Parameter.Modifier);
        Assert.Equal("string", method0Parameter.Type.Name);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((ClassModel)classTypes[1]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(1, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("TryGetValue", calledMethod0.Name);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.DefinitionMethodNames);

        Assert.Equal(2, calledMethod0.ParameterTypes.Count);

        var calledMethod0Parameter = (ParameterModel)calledMethod0.ParameterTypes[0];
        Assert.Equal("", calledMethod0Parameter.Modifier);
        Assert.Equal("string", calledMethod0Parameter.Type.Name);
        Assert.Null(calledMethod0Parameter.DefaultValue);

        var method0Parameter = (ParameterModel)calledMethod0.ParameterTypes[1];
        Assert.Equal("out", method0Parameter.Modifier);
        Assert.Equal("string", method0Parameter.Type.Name);
        Assert.Null(method0Parameter.DefaultValue);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpCalledMethods/ClassWithMethodsThatCallsInnerGenericMethod.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedWithGenericMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var genericMethodModel = ((ClassModel)classTypes[0]).Methods[0];
        Assert.Equal("Method", genericMethodModel.Name);
        Assert.Equal(1, genericMethodModel.ParameterTypes.Count);
        Assert.Equal("T", genericMethodModel.ParameterTypes[0].Type.Name);
        Assert.Equal(0, genericMethodModel.CalledMethods.Count);

        var methodCaller = ((ClassModel)classTypes[0]).Methods[1];
        Assert.Equal("Caller", methodCaller.Name);
        Assert.Empty(methodCaller.ParameterTypes);
        Assert.Equal(3, methodCaller.CalledMethods.Count);

        var calledMethod0 = methodCaller.CalledMethods[0];
        Assert.Equal("Method<int>", calledMethod0.Name);
        Assert.Equal("TopLevel.Bar", calledMethod0.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.DefinitionMethodNames);
        Assert.Equal(1, calledMethod0.ParameterTypes.Count);
        Assert.Equal("int", calledMethod0.ParameterTypes[0].Type.Name);

        var calledMethod1 = methodCaller.CalledMethods[1];
        Assert.Equal("Method", calledMethod1.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.DefinitionMethodNames);
        Assert.Equal(1, calledMethod1.ParameterTypes.Count);
        Assert.Equal("int", calledMethod1.ParameterTypes[0].Type.Name);

        var calledMethod2 = methodCaller.CalledMethods[2];
        Assert.Equal("Method<double>", calledMethod2.Name);
        Assert.Equal("TopLevel.Bar", calledMethod2.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.DefinitionMethodNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        Assert.Equal("double", calledMethod2.ParameterTypes[0].Type.Name);
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithLocalVariableFromTypeof.txt")]
    public void Extract_ShouldHaveNoCalledMethods_WhenProvidedWithTypeofSyntax(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Empty(methodType.CalledMethods);
        }
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithLocalVariableFromNameof.txt")]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithLocalVariableFromNameofOfEnum.txt")]
    public void Extract_ShouldExtractNameof_WhenProvidedWithNameofSyntax(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.CalledMethods.Count);
            Assert.Equal("nameof", methodType.CalledMethods[0].Name);
            Assert.Equal("", methodType.CalledMethods[0].DefinitionClassName);
            Assert.Equal("", methodType.CalledMethods[0].LocationClassName);
            Assert.Empty(methodType.CalledMethods[0].DefinitionMethodNames);
        }
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithAwaitStatement.txt")]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithAwaitStatementWithUnknownClass.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenGivenMethodWithAwaitStatement(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(4, methodType.CalledMethods.Count);
        Assert.Equal("Wait", methodType.CalledMethods[0].Name);
        Assert.Equal("Get", methodType.CalledMethods[1].Name);
        Assert.Equal("Wait", methodType.CalledMethods[2].Name);
        Assert.Equal("Get", methodType.CalledMethods[3].Name);
    }
}
