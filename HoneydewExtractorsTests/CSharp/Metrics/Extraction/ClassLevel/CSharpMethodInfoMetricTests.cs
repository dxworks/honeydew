﻿using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Method;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpMethodInfoMetricTests()
        {
            var compositeVisitor = new CompositeVisitor<ICompilationUnitType>();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
                    {
                        new MethodCallInfoVisitor()
                    })
                })
            }));
            
            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Fact]
        public void Extract_ShouldHaveNoMethods_WhenGivenClassAndRecordsWithFieldsOnly()
        {
            const string fileContent = @"using System;
     
                                      namespace TopLevel
                                      {
                                          public class Foo { public int f; string g; }

                                          public record Bar { Boolean a; string g; int x; float b; }                                        
                                      }";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;

                Assert.Empty(classModel.Methods);
            }
        }

        [Fact]
        public void Extract_ShouldHaveMethods_WhenGivenAClassHierarchy()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                             public abstract class Foo
                                             {
                                                 protected int G(float a)
                                                 {
                                                     return (int) a;
                                                 }

                                                 public virtual bool H()
                                                 {
                                                     return false;
                                                 }

                                                 public abstract int X();
                                             }

                                             public class Bar : Foo
                                             {
                                                 public override int X()
                                                 {
                                                     return 1;
                                                 }

                                                 public override bool H()
                                                 {
                                                     return true;
                                                 }
                                             }                                  
                                      }";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel1 = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel1.Methods.Count);

            Assert.Equal("G", classModel1.Methods[0].Name);
            Assert.Equal("int", classModel1.Methods[0].ReturnType.Name);
            Assert.Equal(1, classModel1.Methods[0].ParameterTypes.Count);
            var parameterModel = (ParameterModel)classModel1.Methods[0].ParameterTypes[0];
            Assert.Equal("float", parameterModel.Name);
            Assert.Equal("", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[0].ContainingTypeName);
            Assert.Equal("protected", classModel1.Methods[0].AccessModifier);
            Assert.Equal("", classModel1.Methods[0].Modifier);
            Assert.Empty(classModel1.Methods[0].CalledMethods);

            Assert.Equal("H", classModel1.Methods[1].Name);
            Assert.Equal("bool", classModel1.Methods[1].ReturnType.Name);
            Assert.Empty(classModel1.Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[1].ContainingTypeName);
            Assert.Equal("public", classModel1.Methods[1].AccessModifier);
            Assert.Equal("virtual", classModel1.Methods[1].Modifier);
            Assert.Empty(classModel1.Methods[1].CalledMethods);

            Assert.Equal("X", classModel1.Methods[2].Name);
            Assert.Equal("int", classModel1.Methods[2].ReturnType.Name);
            Assert.Empty(classModel1.Methods[2].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[2].ContainingTypeName);
            Assert.Equal("public", classModel1.Methods[2].AccessModifier);
            Assert.Equal("abstract", classModel1.Methods[2].Modifier);
            Assert.Empty(classModel1.Methods[2].CalledMethods);

            var classModel2 = (ClassModel)classTypes[1];
            Assert.Equal(2, classModel2.Methods.Count);

            Assert.Equal("X", classModel2.Methods[0].Name);
            Assert.Equal("int", classModel2.Methods[0].ReturnType.Name);
            Assert.Empty(classModel2.Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModel2.Methods[0].ContainingTypeName);
            Assert.Equal("public", classModel2.Methods[0].AccessModifier);
            Assert.Equal("override", classModel2.Methods[0].Modifier);
            Assert.Empty(classModel2.Methods[0].CalledMethods);

            Assert.Equal("H", classModel2.Methods[1].Name);
            Assert.Equal("bool", classModel2.Methods[1].ReturnType.Name);
            Assert.Empty(classModel1.Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModel2.Methods[1].ContainingTypeName);
            Assert.Equal("public", classModel2.Methods[1].AccessModifier);
            Assert.Equal("override", classModel2.Methods[1].Modifier);
            Assert.Empty(classModel2.Methods[1].CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHaveMethodsWithMethodCalls_WhenGivenMethodsThatCallOtherMethods()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                             public abstract class Foo
                                             {
                                                 protected int G(float a)
                                                 {
                                                     return (int) a;
                                                 }

                                                 public virtual bool H()
                                                 {
                                                     return false;
                                                 }
                                             }

                                             public class Bar : Foo
                                             {
                                                 int M() {return 2;}
                                             
                                                 public override bool H()
                                                 {
                                                     if (G(0.5f) == 0) {
                                                         return base.H();
                                                     }
                                                     int x = M();
                                                     
                                                     return true;
                                                 }
                                             }                                  
                                      }";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel1 = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel1.Methods.Count);

            Assert.Equal("G", classModel1.Methods[0].Name);
            Assert.Equal("int", classModel1.Methods[0].ReturnType.Name);
            Assert.Equal(1, classModel1.Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)classModel1.Methods[0].ParameterTypes[0];
            Assert.Equal("float", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[0].ContainingTypeName);
            Assert.Equal("protected", classModel1.Methods[0].AccessModifier);
            Assert.Equal("", classModel1.Methods[0].Modifier);
            Assert.Empty(classModel1.Methods[0].CalledMethods);

            Assert.Equal("H", classModel1.Methods[1].Name);
            Assert.Equal("bool", classModel1.Methods[1].ReturnType.Name);
            Assert.Empty(classModel1.Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[1].ContainingTypeName);
            Assert.Equal("public", classModel1.Methods[1].AccessModifier);
            Assert.Equal("virtual", classModel1.Methods[1].Modifier);
            Assert.Empty(classModel1.Methods[1].CalledMethods);

            var classModel2 = (ClassModel)classTypes[1];
            Assert.Equal(2, classModel2.Methods.Count);

            Assert.Equal("M", classModel2.Methods[0].Name);
            Assert.Equal("int", classModel2.Methods[0].ReturnType.Name);
            Assert.Empty(classModel2.Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModel2.Methods[0].ContainingTypeName);
            Assert.Equal("private", classModel2.Methods[0].AccessModifier);
            Assert.Equal("", classModel2.Methods[0].Modifier);
            Assert.Empty(classModel2.Methods[0].CalledMethods);

            Assert.Equal("H", classModel2.Methods[1].Name);
            Assert.Equal("bool", classModel2.Methods[1].ReturnType.Name);
            Assert.Empty(classModel1.Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModel2.Methods[1].ContainingTypeName);
            Assert.Equal("public", classModel2.Methods[1].AccessModifier);
            Assert.Equal("override", classModel2.Methods[1].Modifier);
            Assert.Equal(3, classModel2.Methods[1].CalledMethods.Count);
            Assert.Equal("G", classModel2.Methods[1].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", classModel2.Methods[1].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, classModel2.Methods[1].CalledMethods[0].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)classModel2.Methods[1].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("float", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal("H", classModel2.Methods[1].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", classModel2.Methods[1].CalledMethods[1].ContainingTypeName);
            Assert.Empty(classModel2.Methods[1].CalledMethods[1].ParameterTypes);
            Assert.Equal("M", classModel2.Methods[1].CalledMethods[2].Name);
            Assert.Equal("TopLevel.Bar", classModel2.Methods[1].CalledMethods[2].ContainingTypeName);
            Assert.Empty(classModel2.Methods[1].CalledMethods[2].ParameterTypes);
        }

        [Fact]
        public void Extract_ShouldHaveMethodsWithMethodCalls_WhenGivenMethodsThatCallStaticMethods()
        {
            const string fileContent = @"using System;
                                      
                                      namespace TopLevel
                                      {
                                         public class Foo
                                         {
                                             public int A(int a)
                                             {
                                                 return a * 2;
                                             }

                                             public int B(int a, int b)
                                             {
                                                 return A(a) + this.A(b);
                                             }
                                         }

                                         public class Bar
                                         {
                                             public int F(int a, int b, string c)
                                             {
                                                 Foo f = new Foo();
                                                 var z = new Foo();
                                                 return f.A(a) + f.B(b, K(c)) + z.A(a);
                                             }

                                             private int K(string s)
                                             {
                                                 return s.Length;
                                             }
                                         }                       
                                      }";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel1 = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel1.Methods.Count);

            Assert.Equal("A", classModel1.Methods[0].Name);
            Assert.Equal("int", classModel1.Methods[0].ReturnType.Name);
            Assert.Equal(1, classModel1.Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)classModel1.Methods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[0].ContainingTypeName);
            Assert.Equal("public", classModel1.Methods[0].AccessModifier);
            Assert.Equal("", classModel1.Methods[0].Modifier);
            Assert.Empty(classModel1.Methods[0].CalledMethods);

            Assert.Equal("B", classModel1.Methods[1].Name);
            Assert.Equal("int", classModel1.Methods[1].ReturnType.Name);
            Assert.Equal(2, classModel1.Methods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)classModel1.Methods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            var parameterModel3 = (ParameterModel)classModel1.Methods[1].ParameterTypes[1];
            Assert.Equal("int", parameterModel3.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[1].ContainingTypeName);
            Assert.Equal("public", classModel1.Methods[1].AccessModifier);
            Assert.Equal("", classModel1.Methods[1].Modifier);
            Assert.Equal(2, classModel1.Methods[1].CalledMethods.Count);
            Assert.Equal("A", classModel1.Methods[1].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[1].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, classModel1.Methods[1].CalledMethods[0].ParameterTypes.Count);
            var parameterModel4 = (ParameterModel)classModel1.Methods[1].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel4.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Null(parameterModel4.DefaultValue);
            Assert.Equal("A", classModel1.Methods[1].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", classModel1.Methods[1].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, classModel1.Methods[1].CalledMethods[1].ParameterTypes.Count);
            var parameterModel5 = (ParameterModel)classModel1.Methods[1].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel5.Name);
            Assert.Equal("", parameterModel5.Modifier);
            Assert.Null(parameterModel5.DefaultValue);

            var classModel2 = (ClassModel)classTypes[1];
            Assert.Equal(2, classModel2.Methods.Count);

            var methodModelF = classModel2.Methods[0];
            Assert.Equal("F", methodModelF.Name);
            Assert.Equal("int", methodModelF.ReturnType.Name);
            Assert.Equal(3, methodModelF.ParameterTypes.Count);
            var parameterModel6 = (ParameterModel)methodModelF.ParameterTypes[0];
            Assert.Equal("int", parameterModel6.Name);
            Assert.Equal("", parameterModel6.Modifier);
            Assert.Null(parameterModel6.DefaultValue);
            var parameterModel7 = (ParameterModel)methodModelF.ParameterTypes[1];
            Assert.Equal("int", parameterModel7.Name);
            Assert.Equal("", parameterModel7.Modifier);
            Assert.Null(parameterModel7.DefaultValue);
            var parameterModel8 = (ParameterModel)methodModelF.ParameterTypes[2];
            Assert.Equal("string", parameterModel8.Name);
            Assert.Equal("", parameterModel8.Modifier);
            Assert.Null(parameterModel8.DefaultValue);
            Assert.Equal("TopLevel.Bar", methodModelF.ContainingTypeName);
            Assert.Equal("public", methodModelF.AccessModifier);
            Assert.Equal("", methodModelF.Modifier);
            Assert.Equal(4, methodModelF.CalledMethods.Count);
            Assert.Equal("A", methodModelF.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, methodModelF.CalledMethods[0].ParameterTypes.Count);
            var parameterModel9 = (ParameterModel)methodModelF.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel9.Name);
            Assert.Equal("", parameterModel9.Modifier);
            Assert.Null(parameterModel9.DefaultValue);
            Assert.Equal("B", methodModelF.CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[1].ContainingTypeName);
            Assert.Equal(2, methodModelF.CalledMethods[1].ParameterTypes.Count);
            var parameterModel10 = (ParameterModel)methodModelF.CalledMethods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel10.Name);
            Assert.Equal("", parameterModel10.Modifier);
            Assert.Null(parameterModel10.DefaultValue);
            var parameterModel11 = (ParameterModel)methodModelF.CalledMethods[1].ParameterTypes[1];
            Assert.Equal("int", parameterModel11.Name);
            Assert.Equal("", parameterModel11.Modifier);
            Assert.Null(parameterModel11.DefaultValue);
            Assert.Equal("K", methodModelF.CalledMethods[2].Name);
            Assert.Equal("TopLevel.Bar", methodModelF.CalledMethods[2].ContainingTypeName);
            Assert.Equal(1, methodModelF.CalledMethods[2].ParameterTypes.Count);
            var parameterModel12 = (ParameterModel)methodModelF.CalledMethods[2].ParameterTypes[0];
            Assert.Equal("string", parameterModel12.Name);
            Assert.Equal("", parameterModel12.Modifier);
            Assert.Null(parameterModel12.DefaultValue);
            Assert.Equal("A", methodModelF.CalledMethods[3].Name);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[3].ContainingTypeName);
            Assert.Equal(1, methodModelF.CalledMethods[3].ParameterTypes.Count);
            var parameterModel13 = (ParameterModel)methodModelF.CalledMethods[3].ParameterTypes[0];
            Assert.Equal("int", parameterModel13.Name);
            Assert.Equal("", parameterModel13.Modifier);
            Assert.Null(parameterModel13.DefaultValue);

            var methodModelK = classModel2.Methods[1];
            Assert.Equal("K", methodModelK.Name);
            Assert.Equal("int", methodModelK.ReturnType.Name);
            Assert.Equal(1, methodModelK.ParameterTypes.Count);
            var parameterModel14 = (ParameterModel)methodModelK.ParameterTypes[0];
            Assert.Equal("string", parameterModel14.Name);
            Assert.Equal("", parameterModel14.Modifier);
            Assert.Null(parameterModel14.DefaultValue);
            Assert.Equal("TopLevel.Bar", methodModelK.ContainingTypeName);
            Assert.Equal("private", methodModelK.AccessModifier);
            Assert.Equal("", methodModelK.Modifier);
            Assert.Empty(methodModelK.CalledMethods);
        }

        [Fact]
        public void
            Extract_ShouldHaveMethodsWithMethodCallsWithParameterModifiers_WhenGivenMethodsThatCallOtherMethodsWithParameterModifiers()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                         {
                                             public void Print()
                                             {
                                                 var a=12;
                                                 F(ref a);
                                                 int b;
                                                 K(out b);
                                                 Z(2);
                                             }
                                             
                                             public int F(ref int a)
                                             {
                                                 var b = a;
                                                 a = 5;
                                                 return b;
                                             }

                                             private int K(out int a)
                                             {
                                                 var c = 6;
                                                 a = 2;
                                                 return c;
                                             }
                                             
                                             private int Z(in int a)
                                             {
                                                 var c = a;
                                                 return c*2;
                                             }
                                         }                                   
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(4, classModel.Methods.Count);
            Assert.Empty(classModel.Constructors);

            var printMethod = classModel.Methods[0];
            var fMethod = classModel.Methods[1];
            var kMethod = classModel.Methods[2];
            var zMethod = classModel.Methods[3];

            Assert.Equal("Print", printMethod.Name);
            Assert.Equal("void", printMethod.ReturnType.Name);
            Assert.Empty(printMethod.ParameterTypes);
            Assert.Equal("TopLevel.Foo", printMethod.ContainingTypeName);
            Assert.Equal("public", printMethod.AccessModifier);
            Assert.Equal("", printMethod.Modifier);
            Assert.Equal(3, printMethod.CalledMethods.Count);

            Assert.Equal("F", printMethod.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, printMethod.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)printMethod.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("ref", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("K", printMethod.CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, printMethod.CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)printMethod.CalledMethods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("out", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);

            Assert.Equal("Z", printMethod.CalledMethods[2].Name);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[2].ContainingTypeName);
            Assert.Equal(1, printMethod.CalledMethods[2].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)printMethod.CalledMethods[2].ParameterTypes[0];
            Assert.Equal("int", parameterModel3.Name);
            Assert.Equal("in", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);


            Assert.Equal("F", fMethod.Name);
            Assert.Equal("int", fMethod.ReturnType.Name);
            Assert.Equal("TopLevel.Foo", fMethod.ContainingTypeName);
            Assert.Equal("public", fMethod.AccessModifier);
            Assert.Equal("", fMethod.Modifier);
            Assert.Empty(fMethod.CalledMethods);
            Assert.Equal(1, fMethod.ParameterTypes.Count);
            var fMethodParameter = (ParameterModel)fMethod.ParameterTypes[0];
            Assert.Equal("int", fMethodParameter.Name);
            Assert.Equal("ref", fMethodParameter.Modifier);
            Assert.Null(fMethodParameter.DefaultValue);

            Assert.Equal("K", kMethod.Name);
            Assert.Equal("int", kMethod.ReturnType.Name);
            Assert.Equal("TopLevel.Foo", kMethod.ContainingTypeName);
            Assert.Equal("private", kMethod.AccessModifier);
            Assert.Equal("", kMethod.Modifier);
            Assert.Empty(kMethod.CalledMethods);
            Assert.Equal(1, kMethod.ParameterTypes.Count);
            var kMethodParameter = (ParameterModel)kMethod.ParameterTypes[0];
            Assert.Equal("int", kMethodParameter.Name);
            Assert.Equal("out", kMethodParameter.Modifier);
            Assert.Null(kMethodParameter.DefaultValue);

            Assert.Equal("Z", zMethod.Name);
            Assert.Equal("int", zMethod.ReturnType.Name);
            Assert.Equal("TopLevel.Foo", zMethod.ContainingTypeName);
            Assert.Equal("private", zMethod.AccessModifier);
            Assert.Equal("", zMethod.Modifier);
            Assert.Empty(zMethod.CalledMethods);
            Assert.Equal(1, zMethod.ParameterTypes.Count);
            var zMethodParameter = (ParameterModel)zMethod.ParameterTypes[0];
            Assert.Equal("int", zMethodParameter.Name);
            Assert.Equal("in", zMethodParameter.Modifier);
            Assert.Null(zMethodParameter.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldExtractNullDefaultValue()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                             public class Bar
                                             {
                                                 int MethodWithNullableDefault(string? p = null) {return 2;}
                                             }                                  
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;
            Assert.Equal("null",
                ((ParameterModel)((ClassModel)classTypes[0]).Methods[0].ParameterTypes[0]).DefaultValue);
        }
    }
}
