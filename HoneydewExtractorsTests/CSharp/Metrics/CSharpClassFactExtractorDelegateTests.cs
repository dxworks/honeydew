using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics
{
    public class CSharpClassFactExtractorDelegateTests
    {
        private readonly CSharpFactExtractor _sut;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpClassFactExtractorDelegateTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                new ParameterSetterVisitor(new List<IParameterVisitor>
                {
                    new ParameterInfoVisitor()
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _sut = new CSharpFactExtractor(compositeVisitor);
        }

        [Fact]
        public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithPrimitiveTypes()
        {
            const string fileContent = @"namespace MyDelegates
                              {
                                public delegate void Delegate1();

                                public delegate void Delegate2(string a);

                                public delegate int Delegate3(double b, char c);
                              }";
            
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

            Assert.Equal(3, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var delegateModel = (DelegateModel)classType;
                Assert.Equal("MyDelegates", delegateModel.ContainingTypeName);
                Assert.Equal(1, delegateModel.BaseTypes.Count);
                Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
                Assert.Equal("delegate", delegateModel.ClassType);
                Assert.Equal("public", delegateModel.AccessModifier);
                Assert.Equal("", delegateModel.Modifier);
                Assert.Empty(delegateModel.Metrics);
            }

            var delegateModel0 = (DelegateModel)classTypes[0];
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
            Assert.Empty(delegateModel0.ParameterTypes);

            var delegateModel1 = (DelegateModel)classTypes[1];
            Assert.Equal("MyDelegates.Delegate2", delegateModel1.Name);
            Assert.Equal("void", delegateModel1.ReturnValue.Type.Name);
            Assert.Equal(1, delegateModel1.ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)delegateModel1.ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("string", parameterModel1.Type.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var delegateModel2 = (DelegateModel)classTypes[2];
            Assert.Equal("MyDelegates.Delegate3", delegateModel2.Name);
            Assert.Equal("int", delegateModel2.ReturnValue.Type.Name);
            Assert.Equal(2, delegateModel2.ParameterTypes.Count);

            var parameterModel2 = (ParameterModel)delegateModel2.ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("double", parameterModel2.Type.Name);
            Assert.Null(parameterModel2.DefaultValue);

            var parameterModel3 = (ParameterModel)delegateModel2.ParameterTypes[1];
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Equal("char", parameterModel3.Type.Name);
            Assert.Null(parameterModel3.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithOtherClasses()
        {
            const string fileContent = @"namespace MyDelegates
                              {
                                public class Class1 {}

                                public delegate void Delegate1(Class1 c);

                                public delegate Class1 Delegate2(ExternClass c);
                              }";
            
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var delegateModel = (DelegateModel)classType;
                Assert.Equal("MyDelegates", delegateModel.ContainingTypeName);
                Assert.Equal(1, delegateModel.BaseTypes.Count);
                Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
                Assert.Equal("delegate", delegateModel.ClassType);
                Assert.Equal("public", delegateModel.AccessModifier);
                Assert.Equal("", delegateModel.Modifier);
                Assert.Empty(delegateModel.Metrics);
            }

            var delegateModel0 = (DelegateModel)classTypes[0];
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
            Assert.Equal(1, delegateModel0.ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)delegateModel0.ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("MyDelegates.Class1", parameterModel1.Type.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var delegateModel1 = (DelegateModel)classTypes[1];
            Assert.Equal("MyDelegates.Delegate2", delegateModel1.Name);
            Assert.Equal("MyDelegates.Class1", delegateModel1.ReturnValue.Type.Name);
            Assert.Equal(1, delegateModel1.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)delegateModel1.ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("ExternClass", parameterModel2.Type.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithParametersWithModifiers()
        {
            const string fileContent = @"namespace MyDelegates
                                          {
                                                public delegate void Delegate1(out int c, in string a, char x = 'a');
                                          }";
            
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var delegateModel0 = (DelegateModel)classTypes[0];
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("MyDelegates", delegateModel0.ContainingTypeName);
            Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
            Assert.Equal(3, delegateModel0.ParameterTypes.Count);

            var parameterModel1 = (ParameterModel)delegateModel0.ParameterTypes[0];
            Assert.Equal("out", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var parameterModel2 = (ParameterModel)delegateModel0.ParameterTypes[1];
            Assert.Equal("in", parameterModel2.Modifier);
            Assert.Equal("string", parameterModel2.Type.Name);
            Assert.Null(parameterModel2.DefaultValue);

            var parameterModel3 = (ParameterModel)delegateModel0.ParameterTypes[2];
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Equal("char", parameterModel3.Type.Name);
            Assert.Equal("'a'", parameterModel3.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesInInnerClasses()
        {
            const string fileContent = @"namespace MyDelegates
                                          {
                                                internal delegate void Delegate1(int a);
                                                class Class1
                                                {
                                                    internal delegate int Delegate2();

                                                    class InnerClass
                                                    {
                                                        internal delegate int Delegate3(string a);    
                                                    }
                                                }
                                          }";
            
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

            Assert.Equal(3, classTypes.Count);


            foreach (var classType in classTypes)
            {
                var delegateModel = (DelegateModel)classType;
                Assert.Equal(1, delegateModel.BaseTypes.Count);
                Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Type.Name);
                Assert.Equal("delegate", delegateModel.ClassType);
                Assert.Equal("internal", delegateModel.AccessModifier);
                Assert.Equal("", delegateModel.Modifier);
                Assert.Empty(delegateModel.Metrics);
            }

            var delegateModel0 = (DelegateModel)classTypes[0];
            Assert.Equal("MyDelegates", delegateModel0.ContainingTypeName);
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("void", delegateModel0.ReturnValue.Type.Name);
            Assert.Equal(1, delegateModel0.ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)delegateModel0.ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var delegateModel1 = (DelegateModel)classTypes[1];
            Assert.Equal("MyDelegates.Class1", delegateModel1.ContainingTypeName);
            Assert.Equal("MyDelegates.Class1.Delegate2", delegateModel1.Name);
            Assert.Equal("int", delegateModel1.ReturnValue.Type.Name);
            Assert.Empty(delegateModel1.ParameterTypes);

            var delegateModel2 = (DelegateModel)classTypes[2];
            Assert.Equal("MyDelegates.Class1.InnerClass", delegateModel2.ContainingTypeName);
            Assert.Equal("MyDelegates.Class1.InnerClass.Delegate3", delegateModel2.Name);
            Assert.Equal("int", delegateModel2.ReturnValue.Type.Name);
            Assert.Equal(1, delegateModel2.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)delegateModel2.ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("string", parameterModel2.Type.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }
    }
}
