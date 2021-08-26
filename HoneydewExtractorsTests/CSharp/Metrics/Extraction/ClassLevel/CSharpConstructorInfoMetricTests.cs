using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Constructor;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Parameter;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpConstructorInfoMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpConstructorInfoMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    new ConstructorCallsVisitor(),
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

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }


        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructors()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                          public class Foo {
                                             
                                             public Foo() {}
     
                                             private Foo(int a) {}

                                             public Foo(string a, int b=2) {}  
                                         }                                        
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Empty(classModel.Methods);
            Assert.Equal(3, classModel.Constructors.Count);

            var noArgConstructor = classModel.Constructors[0];
            Assert.Equal("Foo", noArgConstructor.Name);
            Assert.Empty(noArgConstructor.ParameterTypes);
            Assert.Equal("TopLevel.Foo", noArgConstructor.ContainingTypeName);
            Assert.Equal("public", noArgConstructor.AccessModifier);
            Assert.Equal("", noArgConstructor.Modifier);
            Assert.Empty(noArgConstructor.CalledMethods);

            var intArgConstructor = classModel.Constructors[1];
            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)intArgConstructor.ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal("private", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Empty(intArgConstructor.CalledMethods);

            var stringIntArgConstructor = classModel.Constructors[2];
            Assert.Equal("Foo", stringIntArgConstructor.Name);
            Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)stringIntArgConstructor.ParameterTypes[0];
            Assert.Equal("string", parameterModel2.Type.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            var parameterModel3 = (ParameterModel)stringIntArgConstructor.ParameterTypes[1];
            Assert.Equal("int", parameterModel3.Type.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Equal("2", parameterModel3.DefaultValue);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingTypeName);
            Assert.Equal("public", stringIntArgConstructor.AccessModifier);
            Assert.Equal("", stringIntArgConstructor.Modifier);
            Assert.Empty(stringIntArgConstructor.CalledMethods);
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

                                             public Foo(int a) : this(""value"") { }

                                             public Foo(string a, int b = 2) { }                                             
                                       }                                    
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Constructors.Count);

            var noArgConstructor = classModel.Constructors[0];
            var intArgConstructor = classModel.Constructors[1];
            var stringIntArgConstructor = classModel.Constructors[2];

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
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);


            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)intArgConstructor.ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Type.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal("public", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Equal(1, intArgConstructor.CalledMethods.Count);

            Assert.Equal("Foo", intArgConstructor.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].ContainingTypeName);
            Assert.Equal(2, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("string", parameterModel3.Type.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            var parameterModel4 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[1];
            Assert.Equal("int", parameterModel4.Type.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Equal("2", parameterModel4.DefaultValue);

            Assert.Equal("Foo", stringIntArgConstructor.Name);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingTypeName);
            Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
            var parameterModel5 = (ParameterModel)stringIntArgConstructor.ParameterTypes[0];
            Assert.Equal("string", parameterModel5.Type.Name);
            Assert.Equal("", parameterModel5.Modifier);
            Assert.Null(parameterModel5.DefaultValue);
            var parameterModel6 = (ParameterModel)stringIntArgConstructor.ParameterTypes[1];
            Assert.Equal("int", parameterModel6.Type.Name);
            Assert.Equal("", parameterModel6.Modifier);
            Assert.Equal("2", parameterModel6.DefaultValue);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingTypeName);
            Assert.Equal("public", stringIntArgConstructor.AccessModifier);
            Assert.Equal("", stringIntArgConstructor.Modifier);
            Assert.Empty(stringIntArgConstructor.CalledMethods);
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel1 = (ClassModel)classTypes[0];
            var classModel2 = (ClassModel)classTypes[1];

            Assert.Empty(classModel1.Methods);
            Assert.Empty(classModel2.Methods);

            Assert.Equal(3, classModel1.Constructors.Count);
            Assert.Equal(3, classModel2.Constructors.Count);

            var noArgConstructorBase = classModel1.Constructors[0];
            var intArgConstructorBase = classModel1.Constructors[1];
            var intIntConstructorBase = classModel1.Constructors[2];

            AssertBasicConstructorInfo(noArgConstructorBase, "Foo");
            Assert.Empty(noArgConstructorBase.ParameterTypes);
            Assert.Equal(1, noArgConstructorBase.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorBase.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", noArgConstructorBase.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, noArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)noArgConstructorBase.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
            Assert.Equal(1, intArgConstructorBase.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)intArgConstructorBase.ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Type.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal(1, intArgConstructorBase.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorBase.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructorBase.CalledMethods[0].ContainingTypeName);
            Assert.Equal(2, intArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)intArgConstructorBase.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel3.Type.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            var parameterModel4 = (ParameterModel)intArgConstructorBase.CalledMethods[0].ParameterTypes[1];
            Assert.Equal("int", parameterModel4.Type.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Null(parameterModel4.DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
            Assert.Equal(2, intIntConstructorBase.ParameterTypes.Count);
            var parameterModel5 = (ParameterModel)intIntConstructorBase.ParameterTypes[0];
            Assert.Equal("int", parameterModel5.Type.Name);
            Assert.Equal("", parameterModel5.Modifier);
            Assert.Null(parameterModel5.DefaultValue);
            var parameterModel6 = (ParameterModel)intIntConstructorBase.ParameterTypes[1];
            Assert.Equal("int", parameterModel6.Type.Name);
            Assert.Equal("", parameterModel6.Modifier);
            Assert.Null(parameterModel6.DefaultValue);
            Assert.Empty(intIntConstructorBase.CalledMethods);

            var noArgConstructorChild = classModel2.Constructors[0];
            var intArgConstructorChild = classModel2.Constructors[1];
            var stringIntConstructorBase = classModel2.Constructors[2];

            AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
            Assert.Empty(noArgConstructorChild.ParameterTypes);
            Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", noArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, noArgConstructorChild.CalledMethods[0].ParameterTypes.Count);
            var parameterModel7 = (ParameterModel)noArgConstructorChild.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel7.Type.Name);
            Assert.Equal("", parameterModel7.Modifier);
            Assert.Null(parameterModel7.DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
            var parameterModel8 = (ParameterModel)intArgConstructorChild.ParameterTypes[0];
            Assert.Equal("int", parameterModel8.Type.Name);
            Assert.Equal("", parameterModel8.Modifier);
            Assert.Null(parameterModel8.DefaultValue);
            Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", intArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(2, stringIntConstructorBase.ParameterTypes.Count);
            var parameterModel9 = (ParameterModel)stringIntConstructorBase.ParameterTypes[0];
            Assert.Equal("string", parameterModel9.Type.Name);
            Assert.Equal("", parameterModel9.Modifier);
            Assert.Null(parameterModel9.DefaultValue);
            var parameterModel10 = (ParameterModel)stringIntConstructorBase.ParameterTypes[1];
            Assert.Equal("int", parameterModel10.Type.Name);
            Assert.Equal("in", parameterModel10.Modifier);
            Assert.Equal("52", parameterModel10.DefaultValue);
            Assert.Equal(1, stringIntConstructorBase.CalledMethods.Count);
            Assert.Equal("Bar", stringIntConstructorBase.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Bar", stringIntConstructorBase.CalledMethods[0].ContainingTypeName);
            Assert.Empty(stringIntConstructorBase.CalledMethods[0].ParameterTypes);

            static void AssertBasicConstructorInfo(IMethodSkeletonType constructorModel, string className)
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Empty(classModel.Methods);

            Assert.Equal(2, classModel.Constructors.Count);

            var noArgConstructorChild = classModel.Constructors[0];
            var intArgConstructorChild = classModel.Constructors[1];

            AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
            Assert.Empty(noArgConstructorChild.ParameterTypes);
            Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Empty(noArgConstructorChild.CalledMethods[0].ParameterTypes);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
            var parameterModel = (ParameterModel)intArgConstructorChild.ParameterTypes[0];
            Assert.Equal("int", parameterModel.Type.Name);
            Assert.Equal("", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);
            Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].Name);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].ContainingTypeName);
            Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

            static void AssertBasicConstructorInfo(IMethodSkeletonType constructorModel, string className)
            {
                Assert.Equal(className, constructorModel.Name);
                Assert.Equal($"TopLevel.{className}", constructorModel.ContainingTypeName);
                Assert.Equal("public", constructorModel.AccessModifier);
                Assert.Equal("", constructorModel.Modifier);
            }
        }
    }
}
