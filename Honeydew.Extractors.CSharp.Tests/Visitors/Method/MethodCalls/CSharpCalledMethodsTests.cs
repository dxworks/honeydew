using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.MethodCalls;

public class CSharpCalledMethodsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpCalledMethodsTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IMethodCallType>>
                                {
                                    new MethodCallInfoVisitor()
                                }),
                            new CSharpParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                            {
                                new ParameterInfoVisitor()
                            }),
                            new CSharpReturnValueSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IReturnValueType>>
                                {
                                    new ReturnValueInfoVisitor()
                                })
                        }),
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/StaticMethodCalls.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsStaticMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[1]).Methods[1];
        Assert.Equal("M", methodModelM.Name);
        Assert.Equal(3, methodModelM.CalledMethods.Count);

        var calledMethod1 = methodModelM.CalledMethods[0];
        Assert.Equal("OtherMethod", calledMethod1.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Empty(calledMethod1.ParameterTypes);

        var calledMethod2 = methodModelM.CalledMethods[1];
        Assert.Equal("Method", calledMethod2.Name);
        Assert.Equal("TopLevel.Foo", calledMethod2.DefinitionClassName);
        Assert.Equal("TopLevel.Foo", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.MethodDefinitionNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        var calledMethod2Parameter = (CSharpParameterModel)calledMethod2.ParameterTypes[0];
        Assert.Equal("", calledMethod2Parameter.Modifier);
        Assert.Null(calledMethod2Parameter.DefaultValue);
        Assert.Equal("int", calledMethod2Parameter.Type.Name);

        var calledMethod3 = methodModelM.CalledMethods[2];
        Assert.Equal("Parse", calledMethod3.Name);
        Assert.Equal("int", calledMethod3.DefinitionClassName);
        Assert.Equal("int", calledMethod3.LocationClassName);
        Assert.Empty(calledMethod3.MethodDefinitionNames);
        Assert.Equal(1, calledMethod3.ParameterTypes.Count);
        var calledMethod3Parameter = (CSharpParameterModel)calledMethod3.ParameterTypes[0];
        Assert.Equal("", calledMethod3Parameter.Modifier);
        Assert.Null(calledMethod3Parameter.DefaultValue);
        Assert.Equal("string", calledMethod3Parameter.Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodCallFromUnknownClass.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsStaticMethodsFromUnknownClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[0]).Methods[0];
        Assert.Equal("M", methodModelM.Name);
        Assert.Equal(1, methodModelM.CalledMethods.Count);

        var calledMethod = methodModelM.CalledMethods[0];
        Assert.Equal("Method", calledMethod.Name);
        Assert.Equal("Foo", calledMethod.DefinitionClassName);
        Assert.Equal("Foo", calledMethod.LocationClassName);
        Assert.Empty(calledMethod.MethodDefinitionNames);
        Assert.Equal(1, calledMethod.ParameterTypes.Count);
        var calledMethodParameter = (CSharpParameterModel)calledMethod.ParameterTypes[0];
        Assert.Equal("", calledMethodParameter.Modifier);
        Assert.Equal("System.Int32", calledMethodParameter.Type.Name);
        Assert.Null(calledMethodParameter.DefaultValue);
    }

    [Theory]
    [FileData("TestData/FuncLambdasCalls.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsFuncLambdas(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[0]).Methods[1];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(5, methodModelM.CalledMethods.Count);

        foreach (var calledMethod in methodModelM.CalledMethods)
        {
            Assert.Equal("Other", calledMethod.Name);
            Assert.Equal("TopLevel.Bar", calledMethod.DefinitionClassName);
            Assert.Equal("TopLevel.Bar", calledMethod.LocationClassName);
            Assert.Empty(calledMethod.MethodDefinitionNames);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            var calledMethodParameter = (CSharpParameterModel)calledMethod.ParameterTypes[0];
            Assert.Equal("", calledMethodParameter.Modifier);
            Assert.Null(calledMethodParameter.DefaultValue);
            Assert.Equal("System.Func<int>", calledMethodParameter.Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/ActionLambdasCalls.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsActionLambdas(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[0]).Methods[1];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(4, methodModelM.CalledMethods.Count);

        foreach (var calledMethod in methodModelM.CalledMethods)
        {
            Assert.Equal("Other", calledMethod.Name);
            Assert.Equal("TopLevel.Bar", calledMethod.DefinitionClassName);
            Assert.Equal("TopLevel.Bar", calledMethod.LocationClassName);
            Assert.Empty(calledMethod.MethodDefinitionNames);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            var calledMethodParameter = (CSharpParameterModel)calledMethod.ParameterTypes[0];
            Assert.Equal("", calledMethodParameter.Modifier);
            Assert.Null(calledMethodParameter.DefaultValue);
            Assert.Equal("System.Action<int>", calledMethodParameter.Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodCallsInsideLambdas.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallMethodsInsideLambdas(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[0]).Methods[2];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(4, methodModelM.CalledMethods.Count);


        var calledMethod1 = methodModelM.CalledMethods[0];
        Assert.Equal("Other", calledMethod1.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Equal(1, calledMethod1.ParameterTypes.Count);
        var calledMethod1Parameter = (CSharpParameterModel)calledMethod1.ParameterTypes[0];
        Assert.Equal("", calledMethod1Parameter.Modifier);
        Assert.Null(calledMethod1Parameter.DefaultValue);
        Assert.Equal("System.Action<int>", calledMethod1Parameter.Type.Name);

        var calledMethod2 = methodModelM.CalledMethods[1];
        Assert.Equal("Calc", calledMethod2.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        var calledMethod2Parameter = (CSharpParameterModel)calledMethod2.ParameterTypes[0];
        Assert.Equal("", calledMethod2Parameter.Modifier);
        Assert.Null(calledMethod2Parameter.DefaultValue);
        Assert.Equal("int", calledMethod2Parameter.Type.Name);

        var calledMethod3 = methodModelM.CalledMethods[2];
        Assert.Equal("Other", calledMethod3.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Equal(1, calledMethod3.ParameterTypes.Count);
        var calledMethod3Parameter = (CSharpParameterModel)calledMethod3.ParameterTypes[0];
        Assert.Equal("", calledMethod3Parameter.Modifier);
        Assert.Null(calledMethod3Parameter.DefaultValue);
        Assert.Equal("System.Func<int>", calledMethod3Parameter.Type.Name);

        var calledMethod4 = methodModelM.CalledMethods[3];
        Assert.Equal("Calc", calledMethod4.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Equal(1, calledMethod4.ParameterTypes.Count);
        var calledMethod4Parameter = (CSharpParameterModel)calledMethod4.ParameterTypes[0];
        Assert.Equal("", calledMethod4Parameter.Modifier);
        Assert.Null(calledMethod4Parameter.DefaultValue);
        Assert.Equal("int", calledMethod4Parameter.Type.Name);
    }

    [Theory]
    [FileData("TestData/ChainedMethodCalls.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallHasCalledMethodsChained(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[2]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(5, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("Trim", calledMethod0.Name);
        Assert.Equal("string", calledMethod0.DefinitionClassName);
        Assert.Equal("string", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.MethodDefinitionNames);
        Assert.Empty(calledMethod0.ParameterTypes);

        var calledMethod1 = methodModelM.CalledMethods[1];
        Assert.Equal("GetName", calledMethod1.Name);
        Assert.Equal("TopLevel.Foo", calledMethod1.LocationClassName);
        Assert.Equal("TopLevel.Foo", calledMethod1.DefinitionClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Empty(calledMethod1.ParameterTypes);

        var calledMethod2 = methodModelM.CalledMethods[2];
        Assert.Equal("Build", calledMethod2.Name);
        Assert.Equal("TopLevel.Builder", calledMethod2.DefinitionClassName);
        Assert.Equal("TopLevel.Builder", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.MethodDefinitionNames);
        Assert.Empty(calledMethod2.ParameterTypes);

        var calledMethod3 = methodModelM.CalledMethods[3];
        Assert.Equal("Set", calledMethod3.Name);
        Assert.Equal("TopLevel.Builder", calledMethod3.DefinitionClassName);
        Assert.Equal("TopLevel.Builder", calledMethod3.LocationClassName);
        Assert.Empty(calledMethod3.MethodDefinitionNames);
        Assert.Empty(calledMethod3.ParameterTypes);

        var calledMethod4 = methodModelM.CalledMethods[4];
        Assert.Equal("Create", calledMethod4.Name);
        Assert.Equal("TopLevel.Bar", calledMethod4.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod4.LocationClassName);
        Assert.Empty(calledMethod4.MethodDefinitionNames);
        Assert.Empty(calledMethod4.ParameterTypes);
    }

    [Theory]
    [FileData("TestData/LinqMethodCalls.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsLinqMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[0]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(5, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("ToList", calledMethod0.Name);
        Assert.Equal("System.Linq.Enumerable", calledMethod0.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.MethodDefinitionNames);
        Assert.Empty(calledMethod0.ParameterTypes);

        var calledMethod1 = methodModelM.CalledMethods[1];
        Assert.Equal("Select", calledMethod1.Name);
        Assert.Equal("System.Linq.Enumerable", calledMethod1.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Equal(1, calledMethod1.ParameterTypes.Count);
        var calledMethod1Parameter = (CSharpParameterModel)calledMethod1.ParameterTypes[0];
        Assert.Equal("", calledMethod1Parameter.Modifier);
        Assert.Equal("System.Func<string, string>", calledMethod1Parameter.Type.Name);
        Assert.Null(calledMethod1Parameter.DefaultValue);

        var calledMethod2 = methodModelM.CalledMethods[2];
        Assert.Equal("Skip", calledMethod2.Name);
        Assert.Equal("System.Linq.Enumerable", calledMethod2.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.MethodDefinitionNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        var calledMethod2Parameter = (CSharpParameterModel)calledMethod2.ParameterTypes[0];
        Assert.Equal("", calledMethod2Parameter.Modifier);
        Assert.Equal("int", calledMethod2Parameter.Type.Name);
        Assert.Null(calledMethod2Parameter.DefaultValue);

        var calledMethod3 = methodModelM.CalledMethods[3];
        Assert.Equal("Where", calledMethod3.Name);
        Assert.Equal("System.Linq.Enumerable", calledMethod3.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.List<string>", calledMethod3.LocationClassName);
        Assert.Empty(calledMethod3.MethodDefinitionNames);
        Assert.Equal(1, calledMethod3.ParameterTypes.Count);
        var calledMethod3Parameter = (CSharpParameterModel)calledMethod3.ParameterTypes[0];
        Assert.Equal("", calledMethod3Parameter.Modifier);
        Assert.Equal("System.Func<string, bool>", calledMethod3Parameter.Type.Name);
        Assert.Null(calledMethod3Parameter.DefaultValue);

        var calledMethod4 = methodModelM.CalledMethods[4];
        Assert.Equal("Trim", calledMethod4.Name);
        Assert.Equal("string", calledMethod4.DefinitionClassName);
        Assert.Equal("string", calledMethod4.LocationClassName);
        Assert.Empty(calledMethod4.MethodDefinitionNames);
        Assert.Empty(calledMethod4.ParameterTypes);
    }

    [Theory]
    [FileData("TestData/MethodClassFromCastedObject.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsMethodsFromDictionaryOfACastedObject(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[1]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(1, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("TryGetValue", calledMethod0.Name);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.MethodDefinitionNames);

        Assert.Equal(2, calledMethod0.ParameterTypes.Count);

        var calledMethod0Parameter = (CSharpParameterModel)calledMethod0.ParameterTypes[0];
        Assert.Equal("", calledMethod0Parameter.Modifier);
        Assert.Equal("string", calledMethod0Parameter.Type.Name);
        Assert.Null(calledMethod0Parameter.DefaultValue);

        var method0Parameter = (CSharpParameterModel)calledMethod0.ParameterTypes[1];
        Assert.Equal("out", method0Parameter.Modifier);
        Assert.Equal("string", method0Parameter.Type.Name);
        Assert.Null(method0Parameter.DefaultValue);
    }

    [Theory]
    [FileData("TestData/MethodCallsFromAnotherClass.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsMethodsFromAnotherClassAsProperty(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModelM = ((CSharpClassModel)classTypes[1]).Methods[0];
        Assert.Equal("Method", methodModelM.Name);
        Assert.Equal(1, methodModelM.CalledMethods.Count);

        var calledMethod0 = methodModelM.CalledMethods[0];
        Assert.Equal("TryGetValue", calledMethod0.Name);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.DefinitionClassName);
        Assert.Equal("System.Collections.Generic.Dictionary<string, string>", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.MethodDefinitionNames);

        Assert.Equal(2, calledMethod0.ParameterTypes.Count);

        var calledMethod0Parameter = (CSharpParameterModel)calledMethod0.ParameterTypes[0];
        Assert.Equal("", calledMethod0Parameter.Modifier);
        Assert.Equal("string", calledMethod0Parameter.Type.Name);
        Assert.Null(calledMethod0Parameter.DefaultValue);

        var method0Parameter = (CSharpParameterModel)calledMethod0.ParameterTypes[1];
        Assert.Equal("out", method0Parameter.Modifier);
        Assert.Equal("string", method0Parameter.Type.Name);
        Assert.Null(method0Parameter.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithMethodsThatCallsInnerGenericMethod.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedWithGenericMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var genericMethodModel = ((CSharpClassModel)classTypes[0]).Methods[0];
        Assert.Equal("Method", genericMethodModel.Name);
        Assert.Equal(1, genericMethodModel.ParameterTypes.Count);
        Assert.Equal("T", genericMethodModel.ParameterTypes[0].Type.Name);
        Assert.Equal(0, genericMethodModel.CalledMethods.Count);

        var methodCaller = ((CSharpClassModel)classTypes[0]).Methods[1];
        Assert.Equal("Caller", methodCaller.Name);
        Assert.Empty(methodCaller.ParameterTypes);
        Assert.Equal(3, methodCaller.CalledMethods.Count);

        var calledMethod0 = methodCaller.CalledMethods[0];
        Assert.Equal("Method<int>", calledMethod0.Name);
        Assert.Equal("TopLevel.Bar", calledMethod0.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod0.LocationClassName);
        Assert.Empty(calledMethod0.MethodDefinitionNames);
        Assert.Equal(1, calledMethod0.ParameterTypes.Count);
        Assert.Equal("int", calledMethod0.ParameterTypes[0].Type.Name);

        var calledMethod1 = methodCaller.CalledMethods[1];
        Assert.Equal("Method", calledMethod1.Name);
        Assert.Equal("TopLevel.Bar", calledMethod1.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod1.LocationClassName);
        Assert.Empty(calledMethod1.MethodDefinitionNames);
        Assert.Equal(1, calledMethod1.ParameterTypes.Count);
        Assert.Equal("int", calledMethod1.ParameterTypes[0].Type.Name);

        var calledMethod2 = methodCaller.CalledMethods[2];
        Assert.Equal("Method<double>", calledMethod2.Name);
        Assert.Equal("TopLevel.Bar", calledMethod2.DefinitionClassName);
        Assert.Equal("TopLevel.Bar", calledMethod2.LocationClassName);
        Assert.Empty(calledMethod2.MethodDefinitionNames);
        Assert.Equal(1, calledMethod2.ParameterTypes.Count);
        Assert.Equal("double", calledMethod2.ParameterTypes[0].Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodWithLocalVariableFromTypeof.txt")]
    public void Extract_ShouldHaveNoCalledMethods_WhenProvidedWithTypeofSyntax(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Empty(methodType.CalledMethods);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithLocalVariableFromNameof.txt")]
    [FileData("TestData/MethodWithLocalVariableFromNameofOfEnum.txt")]
    public void Extract_ShouldExtractNameof_WhenProvidedWithNameofSyntax(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            Assert.Equal(1, methodType.CalledMethods.Count);
            Assert.Equal("nameof", methodType.CalledMethods[0].Name);
            Assert.Equal("", methodType.CalledMethods[0].DefinitionClassName);
            Assert.Equal("", methodType.CalledMethods[0].LocationClassName);
            Assert.Empty(methodType.CalledMethods[0].MethodDefinitionNames);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithAwaitStatement.txt")]
    [FileData("TestData/MethodWithAwaitStatementWithUnknownClass.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenGivenMethodWithAwaitStatement(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        var methodType = classModel.Methods[0];
        Assert.Equal(4, methodType.CalledMethods.Count);
        Assert.Equal("Wait", methodType.CalledMethods[0].Name);
        Assert.Equal("Get", methodType.CalledMethods[1].Name);
        Assert.Equal("Wait", methodType.CalledMethods[2].Name);
        Assert.Equal("Get", methodType.CalledMethods[3].Name);
    }

    [Theory]
    [FileData("TestData/MethodCallsInClassHierarchy.txt")]
    public void Extract_ShouldHaveMethodsWithMethodCalls_WhenGivenMethodsThatCallOtherMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel1 = (CSharpClassModel)classTypes[0];
        Assert.Equal(2, classModel1.Methods.Count);

        Assert.Equal("G", classModel1.Methods[0].Name);
        Assert.Equal("int", classModel1.Methods[0].ReturnValue.Type.Name);
        Assert.Equal(1, classModel1.Methods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)classModel1.Methods[0].ParameterTypes[0];
        Assert.Equal("float", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);
        Assert.Equal("protected", classModel1.Methods[0].AccessModifier);
        Assert.Equal("", classModel1.Methods[0].Modifier);
        Assert.Empty(classModel1.Methods[0].CalledMethods);

        Assert.Equal("H", classModel1.Methods[1].Name);
        Assert.Equal("bool", classModel1.Methods[1].ReturnValue.Type.Name);
        Assert.Empty(classModel1.Methods[1].ParameterTypes);
        Assert.Equal("public", classModel1.Methods[1].AccessModifier);
        Assert.Equal("virtual", classModel1.Methods[1].Modifier);
        Assert.Empty(classModel1.Methods[1].CalledMethods);

        var classModel2 = (CSharpClassModel)classTypes[1];
        Assert.Equal(2, classModel2.Methods.Count);

        Assert.Equal("M", classModel2.Methods[0].Name);
        Assert.Equal("int", classModel2.Methods[0].ReturnValue.Type.Name);
        Assert.Empty(classModel2.Methods[0].ParameterTypes);
        Assert.Equal("private", classModel2.Methods[0].AccessModifier);
        Assert.Equal("", classModel2.Methods[0].Modifier);
        Assert.Empty(classModel2.Methods[0].CalledMethods);

        Assert.Equal("H", classModel2.Methods[1].Name);
        Assert.Equal("bool", classModel2.Methods[1].ReturnValue.Type.Name);
        Assert.Empty(classModel1.Methods[1].ParameterTypes);
        Assert.Equal("public", classModel2.Methods[1].AccessModifier);
        Assert.Equal("override", classModel2.Methods[1].Modifier);
        Assert.Equal(3, classModel2.Methods[1].CalledMethods.Count);
        Assert.Equal("G", classModel2.Methods[1].CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", classModel2.Methods[1].CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel2.Methods[1].CalledMethods[0].LocationClassName);
        Assert.Empty(classModel2.Methods[1].CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, classModel2.Methods[1].CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)classModel2.Methods[1].CalledMethods[0].ParameterTypes[0];
        Assert.Equal("float", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        Assert.Equal("H", classModel2.Methods[1].CalledMethods[1].Name);
        Assert.Equal("TopLevel.Foo", classModel2.Methods[1].CalledMethods[1].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel2.Methods[1].CalledMethods[1].LocationClassName);
        Assert.Empty(classModel2.Methods[1].CalledMethods[1].MethodDefinitionNames);
        Assert.Empty(classModel2.Methods[1].CalledMethods[1].ParameterTypes);
        Assert.Equal("M", classModel2.Methods[1].CalledMethods[2].Name);
        Assert.Equal("TopLevel.Bar", classModel2.Methods[1].CalledMethods[2].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", classModel2.Methods[1].CalledMethods[2].LocationClassName);
        Assert.Empty(classModel2.Methods[1].CalledMethods[2].MethodDefinitionNames);
        Assert.Empty(classModel2.Methods[1].CalledMethods[2].ParameterTypes);
    }

    [Theory]
    [FileData("TestData/StaticMethodCallsFromMultipleClasses.txt")]
    public void Extract_ShouldHaveMethodsWithMethodCalls_WhenGivenMethodsThatCallStaticMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel1 = (CSharpClassModel)classTypes[0];
        Assert.Equal(2, classModel1.Methods.Count);

        Assert.Equal("A", classModel1.Methods[0].Name);
        Assert.Equal("int", classModel1.Methods[0].ReturnValue.Type.Name);
        Assert.Equal(1, classModel1.Methods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)classModel1.Methods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);
        Assert.Equal("public", classModel1.Methods[0].AccessModifier);
        Assert.Equal("", classModel1.Methods[0].Modifier);
        Assert.Empty(classModel1.Methods[0].CalledMethods);

        Assert.Equal("B", classModel1.Methods[1].Name);
        Assert.Equal("int", classModel1.Methods[1].ReturnValue.Type.Name);
        Assert.Equal(2, classModel1.Methods[1].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)classModel1.Methods[1].ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        var parameterModel3 = (CSharpParameterModel)classModel1.Methods[1].ParameterTypes[1];
        Assert.Equal("int", parameterModel3.Type.Name);
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Null(parameterModel3.DefaultValue);
        Assert.Equal("public", classModel1.Methods[1].AccessModifier);
        Assert.Equal("", classModel1.Methods[1].Modifier);
        Assert.Equal(2, classModel1.Methods[1].CalledMethods.Count);
        Assert.Equal("A", classModel1.Methods[1].CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", classModel1.Methods[1].CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel1.Methods[1].CalledMethods[0].LocationClassName);
        Assert.Empty(classModel1.Methods[1].CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, classModel1.Methods[1].CalledMethods[0].ParameterTypes.Count);
        var parameterModel4 = (CSharpParameterModel)classModel1.Methods[1].CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel4.Type.Name);
        Assert.Equal("", parameterModel4.Modifier);
        Assert.Null(parameterModel4.DefaultValue);
        Assert.Equal("A", classModel1.Methods[1].CalledMethods[1].Name);
        Assert.Equal("TopLevel.Foo", classModel1.Methods[1].CalledMethods[1].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel1.Methods[1].CalledMethods[1].LocationClassName);
        Assert.Empty(classModel1.Methods[1].CalledMethods[1].MethodDefinitionNames);
        Assert.Equal(1, classModel1.Methods[1].CalledMethods[1].ParameterTypes.Count);
        var parameterModel5 = (CSharpParameterModel)classModel1.Methods[1].CalledMethods[1].ParameterTypes[0];
        Assert.Equal("int", parameterModel5.Type.Name);
        Assert.Equal("", parameterModel5.Modifier);
        Assert.Null(parameterModel5.DefaultValue);

        var classModel2 = (CSharpClassModel)classTypes[1];
        Assert.Equal(2, classModel2.Methods.Count);

        var methodModelF = classModel2.Methods[0];
        Assert.Equal("F", methodModelF.Name);
        Assert.Equal("int", methodModelF.ReturnValue.Type.Name);
        Assert.Equal(3, methodModelF.ParameterTypes.Count);
        var parameterModel6 = (CSharpParameterModel)methodModelF.ParameterTypes[0];
        Assert.Equal("int", parameterModel6.Type.Name);
        Assert.Equal("", parameterModel6.Modifier);
        Assert.Null(parameterModel6.DefaultValue);
        var parameterModel7 = (CSharpParameterModel)methodModelF.ParameterTypes[1];
        Assert.Equal("int", parameterModel7.Type.Name);
        Assert.Equal("", parameterModel7.Modifier);
        Assert.Null(parameterModel7.DefaultValue);
        var parameterModel8 = (CSharpParameterModel)methodModelF.ParameterTypes[2];
        Assert.Equal("string", parameterModel8.Type.Name);
        Assert.Equal("", parameterModel8.Modifier);
        Assert.Null(parameterModel8.DefaultValue);
        Assert.Equal("public", methodModelF.AccessModifier);
        Assert.Equal("", methodModelF.Modifier);
        Assert.Equal(4, methodModelF.CalledMethods.Count);
        Assert.Equal("A", methodModelF.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[0].LocationClassName);
        Assert.Empty(methodModelF.CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, methodModelF.CalledMethods[0].ParameterTypes.Count);
        var parameterModel9 = (CSharpParameterModel)methodModelF.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel9.Type.Name);
        Assert.Equal("", parameterModel9.Modifier);
        Assert.Null(parameterModel9.DefaultValue);
        Assert.Equal("B", methodModelF.CalledMethods[1].Name);
        Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[1].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[1].LocationClassName);
        Assert.Empty(methodModelF.CalledMethods[1].MethodDefinitionNames);
        Assert.Equal(2, methodModelF.CalledMethods[1].ParameterTypes.Count);
        var parameterModel10 = (CSharpParameterModel)methodModelF.CalledMethods[1].ParameterTypes[0];
        Assert.Equal("int", parameterModel10.Type.Name);
        Assert.Equal("", parameterModel10.Modifier);
        Assert.Null(parameterModel10.DefaultValue);
        var parameterModel11 = (CSharpParameterModel)methodModelF.CalledMethods[1].ParameterTypes[1];
        Assert.Equal("int", parameterModel11.Type.Name);
        Assert.Equal("", parameterModel11.Modifier);
        Assert.Null(parameterModel11.DefaultValue);
        Assert.Equal("K", methodModelF.CalledMethods[2].Name);
        Assert.Equal("TopLevel.Bar", methodModelF.CalledMethods[2].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", methodModelF.CalledMethods[2].LocationClassName);
        Assert.Empty(methodModelF.CalledMethods[2].MethodDefinitionNames);
        Assert.Equal(1, methodModelF.CalledMethods[2].ParameterTypes.Count);
        var parameterModel12 = (CSharpParameterModel)methodModelF.CalledMethods[2].ParameterTypes[0];
        Assert.Equal("string", parameterModel12.Type.Name);
        Assert.Equal("", parameterModel12.Modifier);
        Assert.Null(parameterModel12.DefaultValue);
        Assert.Equal("A", methodModelF.CalledMethods[3].Name);
        Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[3].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[3].LocationClassName);
        Assert.Empty(methodModelF.CalledMethods[3].MethodDefinitionNames);
        Assert.Equal(1, methodModelF.CalledMethods[3].ParameterTypes.Count);
        var parameterModel13 = (CSharpParameterModel)methodModelF.CalledMethods[3].ParameterTypes[0];
        Assert.Equal("int", parameterModel13.Type.Name);
        Assert.Equal("", parameterModel13.Modifier);
        Assert.Null(parameterModel13.DefaultValue);

        var methodModelK = classModel2.Methods[1];
        Assert.Equal("K", methodModelK.Name);
        Assert.Equal("int", methodModelK.ReturnValue.Type.Name);
        Assert.Equal(1, methodModelK.ParameterTypes.Count);
        var parameterModel14 = (CSharpParameterModel)methodModelK.ParameterTypes[0];
        Assert.Equal("string", parameterModel14.Type.Name);
        Assert.Equal("", parameterModel14.Modifier);
        Assert.Null(parameterModel14.DefaultValue);
        Assert.Equal("private", methodModelK.AccessModifier);
        Assert.Equal("", methodModelK.Modifier);
        Assert.Empty(methodModelK.CalledMethods);
    }

    [Theory]
    [FileData("TestData/MethodCallsWithParameterModifiers.txt")]
    public void
        Extract_ShouldHaveMethodsWithMethodCallsWithParameterModifiers_WhenGivenMethodsThatCallOtherMethodsWithParameterModifiers(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(4, classModel.Methods.Count);
        Assert.Empty(classModel.Constructors);

        var printMethod = classModel.Methods[0];
        var fMethod = classModel.Methods[1];
        var kMethod = classModel.Methods[2];
        var zMethod = classModel.Methods[3];

        Assert.Equal("Print", printMethod.Name);
        Assert.Equal("void", printMethod.ReturnValue.Type.Name);
        Assert.Empty(printMethod.ParameterTypes);
        Assert.Equal("public", printMethod.AccessModifier);
        Assert.Equal("", printMethod.Modifier);
        Assert.Equal(3, printMethod.CalledMethods.Count);

        Assert.Equal("F", printMethod.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[0].LocationClassName);
        Assert.Empty(printMethod.CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, printMethod.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)printMethod.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("ref", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);

        Assert.Equal("K", printMethod.CalledMethods[1].Name);
        Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[1].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[1].LocationClassName);
        Assert.Empty(printMethod.CalledMethods[1].MethodDefinitionNames);
        Assert.Equal(1, printMethod.CalledMethods[1].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)printMethod.CalledMethods[1].ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("out", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);

        Assert.Equal("Z", printMethod.CalledMethods[2].Name);
        Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[2].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[2].LocationClassName);
        Assert.Empty(printMethod.CalledMethods[2].MethodDefinitionNames);
        Assert.Equal(1, printMethod.CalledMethods[2].ParameterTypes.Count);
        var parameterModel3 = (CSharpParameterModel)printMethod.CalledMethods[2].ParameterTypes[0];
        Assert.Equal("int", parameterModel3.Type.Name);
        Assert.Equal("in", parameterModel3.Modifier);
        Assert.Null(parameterModel3.DefaultValue);


        Assert.Equal("F", fMethod.Name);
        Assert.Equal("int", fMethod.ReturnValue.Type.Name);
        Assert.Equal("public", fMethod.AccessModifier);
        Assert.Equal("", fMethod.Modifier);
        Assert.Empty(fMethod.CalledMethods);
        Assert.Equal(1, fMethod.ParameterTypes.Count);
        var fMethodParameter = (CSharpParameterModel)fMethod.ParameterTypes[0];
        Assert.Equal("int", fMethodParameter.Type.Name);
        Assert.Equal("ref", fMethodParameter.Modifier);
        Assert.Null(fMethodParameter.DefaultValue);

        Assert.Equal("K", kMethod.Name);
        Assert.Equal("int", kMethod.ReturnValue.Type.Name);
        Assert.Equal("private", kMethod.AccessModifier);
        Assert.Equal("", kMethod.Modifier);
        Assert.Empty(kMethod.CalledMethods);
        Assert.Equal(1, kMethod.ParameterTypes.Count);
        var kMethodParameter = (CSharpParameterModel)kMethod.ParameterTypes[0];
        Assert.Equal("int", kMethodParameter.Type.Name);
        Assert.Equal("out", kMethodParameter.Modifier);
        Assert.Null(kMethodParameter.DefaultValue);

        Assert.Equal("Z", zMethod.Name);
        Assert.Equal("int", zMethod.ReturnValue.Type.Name);
        Assert.Equal("private", zMethod.AccessModifier);
        Assert.Equal("", zMethod.Modifier);
        Assert.Empty(zMethod.CalledMethods);
        Assert.Equal(1, zMethod.ParameterTypes.Count);
        var zMethodParameter = (CSharpParameterModel)zMethod.ParameterTypes[0];
        Assert.Equal("int", zMethodParameter.Type.Name);
        Assert.Equal("in", zMethodParameter.Modifier);
        Assert.Null(zMethodParameter.DefaultValue);
    }

    [Theory]
    [FileData("TestData/MethodCallWithHierarchy.txt")]
    public void Extract_ShouldHaveDefinitionClassNameAndLocationClassName_GivenClassHierarchy(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = ((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal(3, method.CalledMethods.Count);

        Assert.Equal("MBase", method.CalledMethods[0].Name);
        Assert.Equal("Namespace1.Middle", method.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", method.CalledMethods[0].LocationClassName);

        Assert.Equal("F", method.CalledMethods[1].Name);
        Assert.Equal("Namespace1.Middle", method.CalledMethods[1].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", method.CalledMethods[1].LocationClassName);

        Assert.Equal("Method", method.CalledMethods[2].Name);
        Assert.Equal("Namespace1.Derived", method.CalledMethods[2].DefinitionClassName);
        Assert.Equal("Namespace1.Derived", method.CalledMethods[2].LocationClassName);
    }

    [Theory]
    [FileData("TestData/MethodCallFromExternClass.txt")]
    public void Extract_ShouldHaveNoMethodDefinitionNames_GivenExternClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = ((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal(1, method.CalledMethods.Count);

        Assert.Equal("Method", method.CalledMethods[0].Name);
        Assert.Equal("Extern", method.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Extern", method.CalledMethods[0].LocationClassName);
        Assert.Empty(method.CalledMethods[0].MethodDefinitionNames);
    }

    [Theory]
    [FileData("TestData/MethodCallsMethodsFromDifferentClass.txt")]
    public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsMethodsFromAFieldOfADifferentClass(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methodModel = ((CSharpClassModel)classTypes[1]).Methods[0];

        Assert.Equal("M", methodModel.Name);
        Assert.Equal(1, methodModel.CalledMethods.Count);
        Assert.Equal("Method", methodModel.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", methodModel.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodModel.CalledMethods[0].LocationClassName);
        Assert.Empty(methodModel.CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, methodModel.CalledMethods[0].ParameterTypes.Count);
        var parameterModel = (CSharpParameterModel)methodModel.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel.Modifier);
        Assert.Null(parameterModel.DefaultValue);
        Assert.Equal("int", parameterModel.Type.Name);
    }
}
