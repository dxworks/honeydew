using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Constructor.Info;

public class CSharpConstructorInfoTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpConstructorInfoTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                new ConstructorCallsVisitor(),
                                new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
                                    new List<ITypeVisitor<IMethodCallType>>
                                    {
                                        new MethodCallInfoVisitor()
                                    }),
                                new CSharpParameterSetterVisitor(_loggerMock.Object,
                                    new List<ITypeVisitor<IParameterType>>
                                    {
                                        new ParameterInfoVisitor()
                                    })
                            })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    [InlineData("interface")]
    public void Extract_ShouldExtractStaticConstructor_WhenProvidedWithDifferentClassType(string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{
        static Class1() {{}}       
    }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.Constructors.Count);

        Assert.Equal("Class1", classModel.Constructors[0].Name);
        Assert.Equal("static", classModel.Constructors[0].Modifier);
        Assert.Equal("", classModel.Constructors[0].AccessModifier);
        Assert.Empty(classModel.Constructors[0].ParameterTypes);
    }

    [Theory]
    [FileData("TestData/MultipleConstructors.txt")]
    public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructors(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Empty(classModel.Methods);
        Assert.Equal(3, classModel.Constructors.Count);

        var noArgConstructor = classModel.Constructors[0];
        Assert.Equal("Foo", noArgConstructor.Name);
        Assert.Empty(noArgConstructor.ParameterTypes);
        Assert.Equal("public", noArgConstructor.AccessModifier);
        Assert.Equal("", noArgConstructor.Modifier);
        Assert.Empty(noArgConstructor.CalledMethods);

        var intArgConstructor = classModel.Constructors[1];
        Assert.Equal("Foo", intArgConstructor.Name);
        Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)intArgConstructor.ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);
        Assert.Equal("private", intArgConstructor.AccessModifier);
        Assert.Equal("", intArgConstructor.Modifier);
        Assert.Empty(intArgConstructor.CalledMethods);

        var stringIntArgConstructor = classModel.Constructors[2];
        Assert.Equal("Foo", stringIntArgConstructor.Name);
        Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)stringIntArgConstructor.ParameterTypes[0];
        Assert.Equal("string", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        var parameterModel3 = (CSharpParameterModel)stringIntArgConstructor.ParameterTypes[1];
        Assert.Equal("int", parameterModel3.Type.Name);
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Equal("2", parameterModel3.DefaultValue);
        Assert.Equal("public", stringIntArgConstructor.AccessModifier);
        Assert.Equal("", stringIntArgConstructor.Modifier);
        Assert.Empty(stringIntArgConstructor.CalledMethods);
    }

    [Theory]
    [FileData("TestData/ConstructorWithThisCalls.txt")]
    public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallEachOther(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Constructors.Count);

        var noArgConstructor = classModel.Constructors[0];
        var intArgConstructor = classModel.Constructors[1];
        var stringIntArgConstructor = classModel.Constructors[2];

        Assert.Equal("Foo", noArgConstructor.Name);
        Assert.Empty(noArgConstructor.ParameterTypes);
        Assert.Equal("public", noArgConstructor.AccessModifier);
        Assert.Equal("", noArgConstructor.Modifier);
        Assert.Equal(1, noArgConstructor.CalledMethods.Count);
        Assert.Equal("Foo", noArgConstructor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", noArgConstructor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", noArgConstructor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, noArgConstructor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)noArgConstructor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);


        Assert.Equal("Foo", intArgConstructor.Name);
        Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)intArgConstructor.ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        Assert.Equal("public", intArgConstructor.AccessModifier);
        Assert.Equal("", intArgConstructor.Modifier);
        Assert.Equal(1, intArgConstructor.CalledMethods.Count);

        Assert.Equal("Foo", intArgConstructor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].LocationClassName);
        Assert.Equal(2, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel3 = (CSharpParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("string", parameterModel3.Type.Name);
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Null(parameterModel3.DefaultValue);
        var parameterModel4 = (CSharpParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[1];
        Assert.Equal("int", parameterModel4.Type.Name);
        Assert.Equal("", parameterModel4.Modifier);
        Assert.Equal("2", parameterModel4.DefaultValue);

        Assert.Equal("Foo", stringIntArgConstructor.Name);
        Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
        var parameterModel5 = (CSharpParameterModel)stringIntArgConstructor.ParameterTypes[0];
        Assert.Equal("string", parameterModel5.Type.Name);
        Assert.Equal("", parameterModel5.Modifier);
        Assert.Null(parameterModel5.DefaultValue);
        var parameterModel6 = (CSharpParameterModel)stringIntArgConstructor.ParameterTypes[1];
        Assert.Equal("int", parameterModel6.Type.Name);
        Assert.Equal("", parameterModel6.Modifier);
        Assert.Equal("2", parameterModel6.DefaultValue);
        Assert.Equal("public", stringIntArgConstructor.AccessModifier);
        Assert.Equal("", stringIntArgConstructor.Modifier);
        Assert.Empty(stringIntArgConstructor.CalledMethods);
    }

    [Theory]
    [FileData("TestData/ConstructorWithBaseCalls.txt")]
    public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallEachOtherAndBaseConstructor(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel1 = (CSharpClassModel)classTypes[0];
        var classModel2 = (CSharpClassModel)classTypes[1];

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
        Assert.Equal("TopLevel.Foo", noArgConstructorBase.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", noArgConstructorBase.CalledMethods[0].LocationClassName);
        Assert.Equal(1, noArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)noArgConstructorBase.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);

        AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
        Assert.Equal(1, intArgConstructorBase.ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)intArgConstructorBase.ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        Assert.Equal(1, intArgConstructorBase.CalledMethods.Count);
        Assert.Equal("Foo", intArgConstructorBase.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", intArgConstructorBase.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", intArgConstructorBase.CalledMethods[0].LocationClassName);
        Assert.Equal(2, intArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
        var parameterModel3 = (CSharpParameterModel)intArgConstructorBase.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel3.Type.Name);
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Null(parameterModel3.DefaultValue);
        var parameterModel4 = (CSharpParameterModel)intArgConstructorBase.CalledMethods[0].ParameterTypes[1];
        Assert.Equal("int", parameterModel4.Type.Name);
        Assert.Equal("", parameterModel4.Modifier);
        Assert.Null(parameterModel4.DefaultValue);

        AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
        Assert.Equal(2, intIntConstructorBase.ParameterTypes.Count);
        var parameterModel5 = (CSharpParameterModel)intIntConstructorBase.ParameterTypes[0];
        Assert.Equal("int", parameterModel5.Type.Name);
        Assert.Equal("", parameterModel5.Modifier);
        Assert.Null(parameterModel5.DefaultValue);
        var parameterModel6 = (CSharpParameterModel)intIntConstructorBase.ParameterTypes[1];
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
        Assert.Equal("TopLevel.Foo", noArgConstructorChild.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", noArgConstructorChild.CalledMethods[0].LocationClassName);
        Assert.Equal(1, noArgConstructorChild.CalledMethods[0].ParameterTypes.Count);
        var parameterModel7 = (CSharpParameterModel)noArgConstructorChild.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel7.Type.Name);
        Assert.Equal("", parameterModel7.Modifier);
        Assert.Null(parameterModel7.DefaultValue);

        AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
        Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
        var parameterModel8 = (CSharpParameterModel)intArgConstructorChild.ParameterTypes[0];
        Assert.Equal("int", parameterModel8.Type.Name);
        Assert.Equal("", parameterModel8.Modifier);
        Assert.Null(parameterModel8.DefaultValue);
        Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
        Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", intArgConstructorChild.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", intArgConstructorChild.CalledMethods[0].LocationClassName);
        Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

        AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
        Assert.Equal(2, stringIntConstructorBase.ParameterTypes.Count);
        var parameterModel9 = (CSharpParameterModel)stringIntConstructorBase.ParameterTypes[0];
        Assert.Equal("string", parameterModel9.Type.Name);
        Assert.Equal("", parameterModel9.Modifier);
        Assert.Null(parameterModel9.DefaultValue);
        var parameterModel10 = (CSharpParameterModel)stringIntConstructorBase.ParameterTypes[1];
        Assert.Equal("int", parameterModel10.Type.Name);
        Assert.Equal("in", parameterModel10.Modifier);
        Assert.Equal("52", parameterModel10.DefaultValue);
        Assert.Equal(1, stringIntConstructorBase.CalledMethods.Count);
        Assert.Equal("Bar", stringIntConstructorBase.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", stringIntConstructorBase.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", stringIntConstructorBase.CalledMethods[0].LocationClassName);
        Assert.Empty(stringIntConstructorBase.CalledMethods[0].ParameterTypes);

        static void AssertBasicConstructorInfo(IMethodSkeletonType constructorModel, string className)
        {
            Assert.Equal(className, constructorModel.Name);
            Assert.Equal("public", constructorModel.AccessModifier);
            Assert.Equal("", constructorModel.Modifier);
        }
    }

    [Theory]
    [FileData("TestData/ConstructorWithCalls.txt")]
    public void
        Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallsBaseConstructor_ButBaseClassIsNotPresentInCompilationUnit(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Empty(classModel.Methods);

        Assert.Equal(2, classModel.Constructors.Count);

        var noArgConstructorChild = classModel.Constructors[0];
        var intArgConstructorChild = classModel.Constructors[1];

        AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
        Assert.Empty(noArgConstructorChild.ParameterTypes);
        Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
        Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].Name);
        Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].LocationClassName);
        Assert.Empty(noArgConstructorChild.CalledMethods[0].ParameterTypes);

        AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
        Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
        var parameterModel = (CSharpParameterModel)intArgConstructorChild.ParameterTypes[0];
        Assert.Equal("int", parameterModel.Type.Name);
        Assert.Equal("", parameterModel.Modifier);
        Assert.Null(parameterModel.DefaultValue);
        Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
        Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].Name);
        Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].LocationClassName);
        Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

        static void AssertBasicConstructorInfo(IMethodSkeletonType constructorModel, string className)
        {
            Assert.Equal(className, constructorModel.Name);
            Assert.Equal("public", constructorModel.AccessModifier);
            Assert.Equal("", constructorModel.Modifier);
        }
    }
}
