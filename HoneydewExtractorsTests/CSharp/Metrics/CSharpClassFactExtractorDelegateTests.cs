using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics
{
    public class CSharpClassFactExtractorDelegateTests
    {
        private readonly CSharpFactExtractor _sut;

        public CSharpClassFactExtractorDelegateTests()
        {
            _sut = new CSharpFactExtractor();
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
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(3, classModels.Count);

            foreach (var delegateModel in classModels)
            {
                Assert.Equal("MyDelegates", delegateModel.Namespace);
                Assert.Equal(1, delegateModel.BaseTypes.Count);
                Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Name);
                Assert.Equal("delegate", delegateModel.ClassType);
                Assert.Equal("public", delegateModel.AccessModifier);
                Assert.Equal("", delegateModel.Modifier);
                Assert.Empty(delegateModel.Constructors);
                Assert.Empty(delegateModel.Fields);
                Assert.Empty(delegateModel.Properties);
                Assert.Empty(delegateModel.Metrics);

                Assert.Equal(1, delegateModel.Methods.Count);
                Assert.Equal(delegateModel.Name, delegateModel.Methods[0].Name);
                Assert.Equal("", delegateModel.Methods[0].Modifier);
                Assert.Equal("", delegateModel.Methods[0].AccessModifier);
                Assert.Empty(delegateModel.Methods[0].CalledMethods);
            }

            var delegateModel0 = classModels[0];
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Methods[0].ContainingTypeName);
            Assert.Equal("void", delegateModel0.Methods[0].ReturnType.Name);
            Assert.Empty(delegateModel0.Methods[0].ParameterTypes);

            var delegateModel1 = classModels[1];
            Assert.Equal("MyDelegates.Delegate2", delegateModel1.Name);
            Assert.Equal("MyDelegates.Delegate2", delegateModel1.Methods[0].ContainingTypeName);
            Assert.Equal("void", delegateModel1.Methods[0].ReturnType.Name);
            Assert.Equal(1, delegateModel1.Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)delegateModel1.Methods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("string", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var delegateModel2 = classModels[2];
            Assert.Equal("MyDelegates.Delegate3", delegateModel2.Name);
            Assert.Equal("MyDelegates.Delegate3", delegateModel2.Methods[0].ContainingTypeName);
            Assert.Equal("int", delegateModel2.Methods[0].ReturnType.Name);
            Assert.Equal(2, delegateModel2.Methods[0].ParameterTypes.Count);

            var parameterModel2 = (ParameterModel)delegateModel2.Methods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("double", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);

            var parameterModel3 = (ParameterModel)delegateModel2.Methods[0].ParameterTypes[1];
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Equal("char", parameterModel3.Name);
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
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(3, classModels.Count);

            for (var i = 1; i < classModels.Count; i++)
            {
                var delegateModel = classModels[i];
                Assert.Equal("MyDelegates", delegateModel.Namespace);
                Assert.Equal(1, delegateModel.BaseTypes.Count);
                Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Name);
                Assert.Equal("delegate", delegateModel.ClassType);
                Assert.Equal("public", delegateModel.AccessModifier);
                Assert.Equal("", delegateModel.Modifier);
                Assert.Empty(delegateModel.Constructors);
                Assert.Empty(delegateModel.Fields);
                Assert.Empty(delegateModel.Properties);
                Assert.Empty(delegateModel.Metrics);

                Assert.Equal(1, delegateModel.Methods.Count);
                Assert.Equal(delegateModel.Name, delegateModel.Methods[0].Name);
                Assert.Equal("", delegateModel.Methods[0].Modifier);
                Assert.Equal("", delegateModel.Methods[0].AccessModifier);
                Assert.Empty(delegateModel.Methods[0].CalledMethods);
            }

            var delegateModel0 = classModels[1];
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Methods[0].ContainingTypeName);
            Assert.Equal("void", delegateModel0.Methods[0].ReturnType.Name);
            Assert.Equal(1, delegateModel0.Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)delegateModel0.Methods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("MyDelegates.Class1", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var delegateModel1 = classModels[2];
            Assert.Equal("MyDelegates.Delegate2", delegateModel1.Name);
            Assert.Equal("MyDelegates.Delegate2", delegateModel1.Methods[0].ContainingTypeName);
            Assert.Equal("MyDelegates.Class1", delegateModel1.Methods[0].ReturnType.Name);
            Assert.Equal(1, delegateModel1.Methods[0].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)delegateModel1.Methods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("ExternClass", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldContainDelegates_WhenParsingTextWithDelegatesWithParametersWithModifiers()
        {
            const string fileContent = @"namespace MyDelegates
                                          {
                                                public delegate void Delegate1(out int c, in string a, char x = 'a');
                                          }";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var delegateModel0 = classModels[0];
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Methods[0].ContainingTypeName);
            Assert.Equal("void", delegateModel0.Methods[0].ReturnType.Name);
            Assert.Equal(3, delegateModel0.Methods[0].ParameterTypes.Count);

            var parameterModel1 = (ParameterModel)delegateModel0.Methods[0].ParameterTypes[0];
            Assert.Equal("out", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var parameterModel2 = (ParameterModel)delegateModel0.Methods[0].ParameterTypes[1];
            Assert.Equal("in", parameterModel2.Modifier);
            Assert.Equal("string", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);

            var parameterModel3 = (ParameterModel)delegateModel0.Methods[0].ParameterTypes[2];
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Equal("char", parameterModel3.Name);
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
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(5, classModels.Count);


            for (var i = 2; i < classModels.Count; i++)
            {
                var delegateModel = classModels[i];
                Assert.Equal(1, delegateModel.BaseTypes.Count);
                Assert.Equal("System.Delegate", delegateModel.BaseTypes[0].Name);
                Assert.Equal("delegate", delegateModel.ClassType);
                Assert.Equal("internal", delegateModel.AccessModifier);
                Assert.Equal("", delegateModel.Modifier);
                Assert.Empty(delegateModel.Constructors);
                Assert.Empty(delegateModel.Fields);
                Assert.Empty(delegateModel.Properties);
                Assert.Empty(delegateModel.Metrics);

                Assert.Equal(1, delegateModel.Methods.Count);
                Assert.Equal(delegateModel.Name, delegateModel.Methods[0].Name);
                Assert.Equal("", delegateModel.Methods[0].Modifier);
                Assert.Equal("", delegateModel.Methods[0].AccessModifier);
                Assert.Empty(delegateModel.Methods[0].CalledMethods);
            }

            var delegateModel0 = classModels[2];
            Assert.Equal("MyDelegates", delegateModel0.Namespace);
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Name);
            Assert.Equal("MyDelegates.Delegate1", delegateModel0.Methods[0].ContainingTypeName);
            Assert.Equal("void", delegateModel0.Methods[0].ReturnType.Name);
            Assert.Equal(1, delegateModel0.Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)delegateModel0.Methods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            var delegateModel1 = classModels[3];
            Assert.Equal("MyDelegates.Class1", delegateModel1.Namespace);
            Assert.Equal("MyDelegates.Class1.Delegate2", delegateModel1.Name);
            Assert.Equal("MyDelegates.Class1.Delegate2", delegateModel1.Methods[0].ContainingTypeName);
            Assert.Equal("int", delegateModel1.Methods[0].ReturnType.Name);
            Assert.Empty(delegateModel1.Methods[0].ParameterTypes);

            var delegateModel2 = classModels[4];
            Assert.Equal("MyDelegates.Class1.InnerClass", delegateModel2.Namespace);
            Assert.Equal("MyDelegates.Class1.InnerClass.Delegate3", delegateModel2.Name);
            Assert.Equal("MyDelegates.Class1.InnerClass.Delegate3", delegateModel2.Methods[0].ContainingTypeName);
            Assert.Equal("int", delegateModel2.Methods[0].ReturnType.Name);
            Assert.Equal(1, delegateModel2.Methods[0].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)delegateModel2.Methods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("string", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }
    }
}
