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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.LocalFunction.Info;

public class LocalFunctionsExtractionTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public LocalFunctionsExtractionTests()
    {
        var calledMethodSetterVisitor = new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor(),
            });
        var parameterSetterVisitor = new CSharpParameterSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IParameterType>>
            {
                new ParameterInfoVisitor()
            });
        var returnValueSetter = new CSharpReturnValueSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IReturnValueType>>
            {
                new ReturnValueInfoVisitor()
            });
        var localFunctionsSetterClassVisitor = new CSharpLocalFunctionsSetterClassVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
            {
                calledMethodSetterVisitor,
                new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                {
                    calledMethodSetterVisitor,
                    parameterSetterVisitor,
                    returnValueSetter,
                }),
                parameterSetterVisitor,
                returnValueSetter,
            });

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
                            calledMethodSetterVisitor,
                            localFunctionsSetterClassVisitor,
                        }),
                        new CSharpConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                localFunctionsSetterClassVisitor
                            }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterPropertyVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    localFunctionsSetterClassVisitor
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/MethodWithVoidLocalFunctionWithNoParameters.txt")]
    public void
        Extract_ShouldExtractLocalFunction_WhenGivenMethodWithOneLocalFunctionThatReturnsVoidAndHasNoParameters(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(1, method.LocalFunctions.Count);
        Assert.Equal("LocalFunction", method.LocalFunctions[0].Name);
        Assert.Equal("", method.LocalFunctions[0].Modifier);
        Assert.Equal("", method.LocalFunctions[0].AccessModifier);
        Assert.Empty(method.LocalFunctions[0].ParameterTypes);
        Assert.Equal("void", method.LocalFunctions[0].ReturnValue.Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodWithLocalFunctionWithReturnValuesAndParameters.txt")]
    public void Extract_ShouldExtractLocalFunction_WhenGivenLocalFunctionWithReturnValueAndParameters(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(1, method.LocalFunctions.Count);
        Assert.Equal("Sum", method.LocalFunctions[0].Name);
        Assert.Equal("", method.LocalFunctions[0].Modifier);
        Assert.Equal("", method.LocalFunctions[0].AccessModifier);
        Assert.Equal(2, method.LocalFunctions[0].ParameterTypes.Count);
        Assert.Equal("int", method.LocalFunctions[0].ParameterTypes[0].Type.Name);
        Assert.Equal("int", method.LocalFunctions[0].ParameterTypes[1].Type.Name);
        Assert.Equal("int", method.LocalFunctions[0].ReturnValue.Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodWithMultipleLocalFunctionsWithModifiers.txt")]
    public void Extract_ShouldExtractMultipleLocalFunctions_WhenGivenLocalFunctionsWithModifiers(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(3, method.LocalFunctions.Count);

        var localFunction = method.LocalFunctions[0];
        Assert.Equal("Function", localFunction.Name);
        Assert.Equal("async", localFunction.Modifier);
        Assert.Equal(1, localFunction.ParameterTypes.Count);
        Assert.Equal("int", localFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("System.Threading.Tasks.Task<int>", localFunction.ReturnValue.Type.Name);

        var staticFunction = method.LocalFunctions[1];
        Assert.Equal("StaticFunction", staticFunction.Name);
        Assert.Equal("static", staticFunction.Modifier);
        Assert.Empty(staticFunction.ParameterTypes);
        Assert.Equal("System.Threading.Tasks.Task<int>", staticFunction.ReturnValue.Type.Name);

        var externFunction = method.LocalFunctions[2];
        Assert.Equal("ExternFunction", externFunction.Name);
        Assert.Equal("static extern", externFunction.Modifier);
        Assert.Equal(1, externFunction.ParameterTypes.Count);
        Assert.Equal("string", externFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", externFunction.ReturnValue.Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodWithLocalFunction_CyclomaticComplexity.txt")]
    public void
        Extract_ShouldExtractLocalFunctionCyclomaticComplexity_WhenGivenLocalFunctionWithHighCyclomaticComplexity(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(1, method.LocalFunctions.Count);
        Assert.Equal("Function", method.LocalFunctions[0].Name);
        Assert.Equal(11, method.LocalFunctions[0].CyclomaticComplexity);
    }

    [Theory]
    [FileData("TestData/MethodPropertyConstructorWithLocalFunction.txt")]
    public void Extract_ShouldExtractLocalFunction_WhenGivenMethodPropertyConstructorWithLocalFunction(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];
        var constructor = (CSharpConstructorModel)((CSharpClassModel)classTypes[0]).Constructors[0];
        var property = (CSharpPropertyModel)((CSharpClassModel)classTypes[0]).Properties[0];
        var eventProperty = (CSharpPropertyModel)((CSharpClassModel)classTypes[0]).Properties[1];

        var localFunctions = new[]
        {
            method.LocalFunctions[0],
            constructor.LocalFunctions[0],
            property.Accessors[0].LocalFunctions[0],
            eventProperty.Accessors[0].LocalFunctions[0],
            eventProperty.Accessors[1].LocalFunctions[0],
        };

        foreach (var localFunction in localFunctions)
        {
            Assert.Equal("Function", localFunction.Name);
            Assert.Equal("static", localFunction.Modifier);
            Assert.Equal(1, localFunction.ParameterTypes.Count);
            Assert.Equal("int", localFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("string", localFunction.ReturnValue.Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithMultipleLocalFunctions_ThatAreCalled.txt")]
    public void Extract_ShouldExtractLocalFunctionAndUsages_WhenGivenMethodWithMultipleLocalFunctions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(4, method.LocalFunctions.Count);

        foreach (var function in method.LocalFunctions)
        {
            Assert.Equal("", function.Modifier);
            Assert.Equal("", function.AccessModifier);
        }

        foreach (var calledMethod in method.CalledMethods)
        {
            Assert.Equal("Namespace1.Class1", calledMethod.DefinitionClassName);
            Assert.Equal("Namespace1.Class1", calledMethod.LocationClassName);
            Assert.Equal("Method(int, int, int)", calledMethod.MethodDefinitionNames[0]);
        }


        Assert.Equal(8, method.CalledMethods.Count);


        var calledMethod1 = method.CalledMethods[0];
        Assert.Equal("Sum", calledMethod1.Name);
        Assert.Equal(2, calledMethod1.ParameterTypes.Count);
        Assert.Equal("int", calledMethod1.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calledMethod1.ParameterTypes[1].Type.Name);

        var calledMethod2 = method.CalledMethods[1];
        Assert.Equal("Sum", calledMethod2.Name);
        Assert.Equal(2, calledMethod2.ParameterTypes.Count);
        Assert.Equal("int", calledMethod2.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calledMethod2.ParameterTypes[1].Type.Name);

        var calledMethod3 = method.CalledMethods[2];
        Assert.Equal("CString", calledMethod3.Name);
        Assert.Empty(calledMethod3.ParameterTypes);

        var calledMethod4 = method.CalledMethods[3];
        Assert.Equal("Print", calledMethod4.Name);
        Assert.Equal(1, calledMethod4.ParameterTypes.Count);
        Assert.Equal("string", calledMethod4.ParameterTypes[0].Type.Name);

        var calledMethod5 = method.CalledMethods[4];
        Assert.Equal("Print", calledMethod5.Name);
        Assert.Equal(1, calledMethod5.ParameterTypes.Count);
        Assert.Equal("string", calledMethod5.ParameterTypes[0].Type.Name);

        var calledMethod6 = method.CalledMethods[5];
        Assert.Equal("Stringify", calledMethod6.Name);
        Assert.Equal(1, calledMethod6.ParameterTypes.Count);
        Assert.Equal("int", calledMethod6.ParameterTypes[0].Type.Name);

        var calledMethod7 = method.CalledMethods[6];
        Assert.Equal("Stringify", calledMethod7.Name);
        Assert.Equal(1, calledMethod7.ParameterTypes.Count);
        Assert.Equal("int", calledMethod7.ParameterTypes[0].Type.Name);

        var calledMethod8 = method.CalledMethods[7];
        Assert.Equal("Print", calledMethod8.Name);
        Assert.Equal(1, calledMethod8.ParameterTypes.Count);
        Assert.Equal("string", calledMethod8.ParameterTypes[0].Type.Name);
    }

    [Theory]
    [FileData("TestData/MethodWithImbricatedLocalFunctions.txt")]
    public void Extract_ShouldExtractLocalFunctions_WhenGivenMethodWithImbricatedLocalFunctions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(1, method.LocalFunctions.Count);

        var stringSumFunction = (CSharpMethodModel)method.LocalFunctions[0];
        Assert.Equal("StringSum", stringSumFunction.Name);
        Assert.Equal("string", stringSumFunction.ReturnValue!.Type.Name);
        Assert.Equal(2, stringSumFunction.ParameterTypes.Count);
        Assert.Equal("int", stringSumFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringSumFunction.ParameterTypes[1].Type.Name);
        Assert.Equal(2, stringSumFunction.LocalFunctions.Count);

        var sumFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[0];
        Assert.Equal("Sum", sumFunction.Name);
        Assert.Equal("int", sumFunction.ReturnValue!.Type.Name);
        Assert.Equal(2, sumFunction.ParameterTypes.Count);
        Assert.Equal("int", sumFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", sumFunction.ParameterTypes[1].Type.Name);
        Assert.Equal(1, sumFunction.LocalFunctions.Count);

        var doubledFunction = (CSharpMethodModel)sumFunction.LocalFunctions[0];
        Assert.Equal("Doubled", doubledFunction.Name);
        Assert.Equal("int", doubledFunction.ReturnValue!.Type.Name);
        Assert.Equal(1, doubledFunction.ParameterTypes.Count);
        Assert.Equal("int", doubledFunction.ParameterTypes[0].Type.Name);
        Assert.Empty(doubledFunction.LocalFunctions);


        var stringifyFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[1];
        Assert.Equal("Stringify", stringifyFunction.Name);
        Assert.Equal("string", stringifyFunction.ReturnValue!.Type.Name);
        Assert.Equal(2, stringifyFunction.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringifyFunction.ParameterTypes[1].Type.Name);
        Assert.Equal(2, stringifyFunction.LocalFunctions.Count);

        var calculateFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[0];
        Assert.Equal("Calculate", calculateFunction.Name);
        Assert.Equal("int", calculateFunction.ReturnValue!.Type.Name);
        Assert.Equal(2, calculateFunction.ParameterTypes.Count);
        Assert.Equal("int", calculateFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calculateFunction.ParameterTypes[1].Type.Name);
        Assert.Empty(calculateFunction.LocalFunctions);

        var stringifyNumberFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[1];
        Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
        Assert.Equal("string", stringifyNumberFunction.ReturnValue!.Type.Name);
        Assert.Equal(1, stringifyNumberFunction.ParameterTypes.Count);
        Assert.Equal("int", stringifyNumberFunction.ParameterTypes[0].Type.Name);
        Assert.Empty(stringifyNumberFunction.LocalFunctions);
    }

    [Theory]
    [FileData("TestData/MethodWithImbricatedLocalFunctions.txt")]
    public void Extract_ShouldExtractLocalFunctionsWithCalledMethods_WhenGivenMethodWithImbricatedLocalFunctions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var method = (CSharpMethodModel)((CSharpClassModel)classTypes[0]).Methods[0];

        Assert.Equal("Method", method.Name);
        Assert.Equal(1, method.LocalFunctions.Count);

        var stringSumFunction = (CSharpMethodModel)method.LocalFunctions[0];
        Assert.Equal("StringSum", stringSumFunction.Name);
        Assert.Equal(2, stringSumFunction.LocalFunctions.Count);
        Assert.Equal(1, stringSumFunction.CalledMethods.Count);
        Assert.Equal("Stringify", stringSumFunction.CalledMethods[0].Name);
        Assert.Equal("Namespace1.Class1", stringSumFunction.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringSumFunction.CalledMethods[0].LocationClassName);
        Assert.Equal("Method(int, int)", stringSumFunction.CalledMethods[0].MethodDefinitionNames[0]);
        Assert.Equal("StringSum(int, int)", stringSumFunction.CalledMethods[0].MethodDefinitionNames[1]);
        Assert.Equal(2, stringSumFunction.CalledMethods[0].ParameterTypes.Count);
        Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[1].Type.Name);

        var sumFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[0];
        Assert.Equal("Sum", sumFunction.Name);
        Assert.Equal(1, sumFunction.LocalFunctions.Count);
        Assert.Equal(2, sumFunction.CalledMethods.Count);
        foreach (var calledMethod in sumFunction.CalledMethods)
        {
            Assert.Equal("Doubled", calledMethod.Name);
            Assert.Equal("Namespace1.Class1", calledMethod.DefinitionClassName);
            Assert.Equal("Namespace1.Class1", calledMethod.LocationClassName);
            Assert.Equal("Method(int, int)", calledMethod.MethodDefinitionNames[0]);
            Assert.Equal("StringSum(int, int)", calledMethod.MethodDefinitionNames[1]);
            Assert.Equal("Sum(int, int)", calledMethod.MethodDefinitionNames[2]);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            Assert.Equal("int", calledMethod.ParameterTypes[0].Type.Name);
        }

        var doubledFunction = (CSharpMethodModel)sumFunction.LocalFunctions[0];
        Assert.Equal("Doubled", doubledFunction.Name);
        Assert.Empty(doubledFunction.LocalFunctions);
        Assert.Empty(doubledFunction.CalledMethods);


        var stringifyFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[1];
        Assert.Equal("Stringify", stringifyFunction.Name);
        Assert.Equal(2, stringifyFunction.LocalFunctions.Count);
        Assert.Equal(2, stringifyFunction.CalledMethods.Count);

        var stringifyFunctionCalledMethod1 = stringifyFunction.CalledMethods[0];
        Assert.Equal("StringifyNumber", stringifyFunctionCalledMethod1.Name);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod1.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod1.LocationClassName);
        Assert.Equal("Method(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[0]);
        Assert.Equal("StringSum(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[1]);
        Assert.Equal("Stringify(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[2]);
        Assert.Equal(1, stringifyFunctionCalledMethod1.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunctionCalledMethod1.ParameterTypes[0].Type.Name);

        var stringifyFunctionCalledMethod2 = stringifyFunction.CalledMethods[1];
        Assert.Equal("Calculate", stringifyFunctionCalledMethod2.Name);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod2.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod2.LocationClassName);
        Assert.Equal("Method(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[0]);
        Assert.Equal("StringSum(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[1]);
        Assert.Equal("Stringify(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[2]);
        Assert.Equal(2, stringifyFunctionCalledMethod2.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[1].Type.Name);


        var calculateFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[0];
        Assert.Equal("Calculate", calculateFunction.Name);
        Assert.Empty(calculateFunction.LocalFunctions);
        Assert.Equal(1, calculateFunction.CalledMethods.Count);

        var calculateFunctionCalledMethod = calculateFunction.CalledMethods[0];
        Assert.Equal("Sum", calculateFunctionCalledMethod.Name);
        Assert.Equal("Namespace1.Class1", calculateFunctionCalledMethod.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", calculateFunctionCalledMethod.LocationClassName);
        Assert.Equal("Method(int, int)", calculateFunctionCalledMethod.MethodDefinitionNames[0]);
        Assert.Equal("StringSum(int, int)", calculateFunctionCalledMethod.MethodDefinitionNames[1]);
        Assert.Equal(2, calculateFunctionCalledMethod.ParameterTypes.Count);
        Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[1].Type.Name);


        var stringifyNumberFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[1];
        Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
        Assert.Empty(stringifyNumberFunction.LocalFunctions);
        Assert.Equal(1, stringifyNumberFunction.CalledMethods.Count);

        var stringifyNumberFunctionCalledMethod = stringifyNumberFunction.CalledMethods[0];
        Assert.Equal("ToString", stringifyNumberFunctionCalledMethod.Name);
        Assert.Equal("int", stringifyNumberFunctionCalledMethod.DefinitionClassName);
        Assert.Equal("int", stringifyNumberFunctionCalledMethod.LocationClassName);
        Assert.Empty(stringifyNumberFunctionCalledMethod.MethodDefinitionNames);
        Assert.Empty(stringifyNumberFunctionCalledMethod.ParameterTypes);
    }

    [Theory]
    [FileData("TestData/PropertyWithImbricatedLocalFunctions.txt")]
    [FileData("TestData/EventPropertyWithImbricatedLocalFunctions.txt")]
    public void Extract_ShouldExtractLocalFunctions_WhenGivenPropertyWithImbricatedLocalFunctions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var property = (CSharpPropertyModel)((CSharpClassModel)classTypes[0]).Properties[0];

        Assert.Equal("Value", property.Name);
        Assert.Equal(1, property.Accessors[0].LocalFunctions.Count);

        var stringSumFunction = property.Accessors[0].LocalFunctions[0];
        Assert.Equal("StringSum", stringSumFunction.Name);
        Assert.Equal("string", stringSumFunction.ReturnValue.Type.Name);
        Assert.Equal(2, stringSumFunction.ParameterTypes.Count);
        Assert.Equal("int", stringSumFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringSumFunction.ParameterTypes[1].Type.Name);
        Assert.Equal(2, stringSumFunction.LocalFunctions.Count);


        var sumFunction = stringSumFunction.LocalFunctions[0];
        Assert.Equal("Sum", sumFunction.Name);
        Assert.Equal("int", sumFunction.ReturnValue.Type.Name);
        Assert.Equal(2, sumFunction.ParameterTypes.Count);
        Assert.Equal("int", sumFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", sumFunction.ParameterTypes[1].Type.Name);
        Assert.Equal(1, sumFunction.LocalFunctions.Count);

        var doubledFunction = sumFunction.LocalFunctions[0];
        Assert.Equal("Doubled", doubledFunction.Name);
        Assert.Equal("int", doubledFunction.ReturnValue.Type.Name);
        Assert.Equal(1, doubledFunction.ParameterTypes.Count);
        Assert.Equal("int", doubledFunction.ParameterTypes[0].Type.Name);
        Assert.Empty(doubledFunction.LocalFunctions);


        var stringifyFunction = stringSumFunction.LocalFunctions[1];
        Assert.Equal("Stringify", stringifyFunction.Name);
        Assert.Equal("string", stringifyFunction.ReturnValue.Type.Name);
        Assert.Equal(2, stringifyFunction.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringifyFunction.ParameterTypes[1].Type.Name);
        Assert.Equal(2, stringifyFunction.LocalFunctions.Count);

        var calculateFunction = stringifyFunction.LocalFunctions[0];
        Assert.Equal("Calculate", calculateFunction.Name);
        Assert.Equal("int", calculateFunction.ReturnValue.Type.Name);
        Assert.Equal(2, calculateFunction.ParameterTypes.Count);
        Assert.Equal("int", calculateFunction.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calculateFunction.ParameterTypes[1].Type.Name);
        Assert.Empty(calculateFunction.LocalFunctions);

        var stringifyNumberFunction = stringifyFunction.LocalFunctions[1];
        Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
        Assert.Equal("string", stringifyNumberFunction.ReturnValue.Type.Name);
        Assert.Equal(1, stringifyNumberFunction.ParameterTypes.Count);
        Assert.Equal("int", stringifyNumberFunction.ParameterTypes[0].Type.Name);
        Assert.Empty(stringifyNumberFunction.LocalFunctions);
    }

    [Theory]
    [FileData("TestData/PropertyWithImbricatedLocalFunctions.txt")]
    public void Extract_ShouldExtractLocalFunctionsWithCalledMethods_WhenGivenPropertyWithImbricatedLocalFunctions(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var property = (CSharpPropertyModel)((CSharpClassModel)classTypes[0]).Properties[0];

        Assert.Equal("Value", property.Name);
        Assert.Equal(1, property.Accessors[0].LocalFunctions.Count);

        var stringSumFunction = property.Accessors[0].LocalFunctions[0];
        Assert.Equal("StringSum", stringSumFunction.Name);
        Assert.Equal(2, stringSumFunction.LocalFunctions.Count);
        Assert.Equal(1, stringSumFunction.CalledMethods.Count);
        Assert.Equal("Stringify", stringSumFunction.CalledMethods[0].Name);
        Assert.Equal("Namespace1.Class1", stringSumFunction.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringSumFunction.CalledMethods[0].LocationClassName);
        Assert.Equal("Value", stringSumFunction.CalledMethods[0].MethodDefinitionNames[0]);
        Assert.Equal("set", stringSumFunction.CalledMethods[0].MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", stringSumFunction.CalledMethods[0].MethodDefinitionNames[2]);
        Assert.Equal(2, stringSumFunction.CalledMethods[0].ParameterTypes.Count);
        Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[1].Type.Name);

        var sumFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[0];
        Assert.Equal("Sum", sumFunction.Name);
        Assert.Equal(1, sumFunction.LocalFunctions.Count);
        Assert.Equal(2, sumFunction.CalledMethods.Count);
        foreach (var calledMethod in sumFunction.CalledMethods)
        {
            Assert.Equal("Doubled", calledMethod.Name);
            Assert.Equal("Namespace1.Class1", calledMethod.DefinitionClassName);
            Assert.Equal("Namespace1.Class1", calledMethod.LocationClassName);
            Assert.Equal("Value", calledMethod.MethodDefinitionNames[0]);
            Assert.Equal("set", calledMethod.MethodDefinitionNames[1]);
            Assert.Equal("StringSum(int, int)", calledMethod.MethodDefinitionNames[2]);
            Assert.Equal("Sum(int, int)", calledMethod.MethodDefinitionNames[3]);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            Assert.Equal("int", calledMethod.ParameterTypes[0].Type.Name);
        }

        var doubledFunction = (CSharpMethodModel)sumFunction.LocalFunctions[0];
        Assert.Equal("Doubled", doubledFunction.Name);
        Assert.Empty(doubledFunction.LocalFunctions);
        Assert.Empty(doubledFunction.CalledMethods);


        var stringifyFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[1];
        Assert.Equal("Stringify", stringifyFunction.Name);
        Assert.Equal(2, stringifyFunction.LocalFunctions.Count);
        Assert.Equal(2, stringifyFunction.CalledMethods.Count);

        var stringifyFunctionCalledMethod1 = stringifyFunction.CalledMethods[0];
        Assert.Equal("StringifyNumber", stringifyFunctionCalledMethod1.Name);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod1.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod1.LocationClassName);
        Assert.Equal("Value", stringifyFunctionCalledMethod1.MethodDefinitionNames[0]);
        Assert.Equal("set", stringifyFunctionCalledMethod1.MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[2]);
        Assert.Equal("Stringify(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[3]);
        Assert.Equal(1, stringifyFunctionCalledMethod1.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunctionCalledMethod1.ParameterTypes[0].Type.Name);

        var stringifyFunctionCalledMethod2 = stringifyFunction.CalledMethods[1];
        Assert.Equal("Calculate", stringifyFunctionCalledMethod2.Name);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod2.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod2.LocationClassName);
        Assert.Equal("Value", stringifyFunctionCalledMethod2.MethodDefinitionNames[0]);
        Assert.Equal("set", stringifyFunctionCalledMethod2.MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[2]);
        Assert.Equal("Stringify(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[3]);
        Assert.Equal(2, stringifyFunctionCalledMethod2.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[1].Type.Name);


        var calculateFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[0];
        Assert.Equal("Calculate", calculateFunction.Name);
        Assert.Empty(calculateFunction.LocalFunctions);
        Assert.Equal(1, calculateFunction.CalledMethods.Count);

        var calculateFunctionCalledMethod = calculateFunction.CalledMethods[0];
        Assert.Equal("Sum", calculateFunctionCalledMethod.Name);
        Assert.Equal("Namespace1.Class1", calculateFunctionCalledMethod.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", calculateFunctionCalledMethod.LocationClassName);
        Assert.Equal("Value", calculateFunctionCalledMethod.MethodDefinitionNames[0]);
        Assert.Equal("set", calculateFunctionCalledMethod.MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", calculateFunctionCalledMethod.MethodDefinitionNames[2]);
        Assert.Equal(2, calculateFunctionCalledMethod.ParameterTypes.Count);
        Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[1].Type.Name);


        var stringifyNumberFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[1];
        Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
        Assert.Empty(stringifyNumberFunction.LocalFunctions);
        Assert.Equal(1, stringifyNumberFunction.CalledMethods.Count);

        var stringifyNumberFunctionCalledMethod = stringifyNumberFunction.CalledMethods[0];
        Assert.Equal("ToString", stringifyNumberFunctionCalledMethod.Name);
        Assert.Equal("int", stringifyNumberFunctionCalledMethod.DefinitionClassName);
        Assert.Equal("int", stringifyNumberFunctionCalledMethod.LocationClassName);
        Assert.Empty(stringifyNumberFunctionCalledMethod.MethodDefinitionNames);
        Assert.Empty(stringifyNumberFunctionCalledMethod.ParameterTypes);
    }

    [Theory]
    [FileData("TestData/EventPropertyWithImbricatedLocalFunctions.txt")]
    public void
        Extract_ShouldExtractLocalFunctionsWithCalledMethods_WhenGivenEventPropertyWithImbricatedLocalFunctions(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var property = (CSharpPropertyModel)((CSharpClassModel)classTypes[0]).Properties[0];

        Assert.Equal("Value", property.Name);
        Assert.Equal(1, property.Accessors[0].LocalFunctions.Count);

        var stringSumFunction = property.Accessors[0].LocalFunctions[0];
        Assert.Equal("StringSum", stringSumFunction.Name);
        Assert.Equal(2, stringSumFunction.LocalFunctions.Count);
        Assert.Equal(1, stringSumFunction.CalledMethods.Count);
        Assert.Equal("Stringify", stringSumFunction.CalledMethods[0].Name);
        Assert.Equal("Namespace1.Class1", stringSumFunction.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringSumFunction.CalledMethods[0].LocationClassName);
        Assert.Equal("Value", stringSumFunction.CalledMethods[0].MethodDefinitionNames[0]);
        Assert.Equal("add", stringSumFunction.CalledMethods[0].MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", stringSumFunction.CalledMethods[0].MethodDefinitionNames[2]);
        Assert.Equal(2, stringSumFunction.CalledMethods[0].ParameterTypes.Count);
        Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[1].Type.Name);

        var sumFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[0];
        Assert.Equal("Sum", sumFunction.Name);
        Assert.Equal(1, sumFunction.LocalFunctions.Count);
        Assert.Equal(2, sumFunction.CalledMethods.Count);
        foreach (var calledMethod in sumFunction.CalledMethods)
        {
            Assert.Equal("Doubled", calledMethod.Name);
            Assert.Equal("Namespace1.Class1", calledMethod.DefinitionClassName);
            Assert.Equal("Namespace1.Class1", calledMethod.LocationClassName);
            Assert.Equal("Value", calledMethod.MethodDefinitionNames[0]);
            Assert.Equal("add", calledMethod.MethodDefinitionNames[1]);
            Assert.Equal("StringSum(int, int)", calledMethod.MethodDefinitionNames[2]);
            Assert.Equal("Sum(int, int)", calledMethod.MethodDefinitionNames[3]);
            Assert.Equal(1, calledMethod.ParameterTypes.Count);
            Assert.Equal("int", calledMethod.ParameterTypes[0].Type.Name);
        }

        var doubledFunction = (CSharpMethodModel)sumFunction.LocalFunctions[0];
        Assert.Equal("Doubled", doubledFunction.Name);
        Assert.Empty(doubledFunction.LocalFunctions);
        Assert.Empty(doubledFunction.CalledMethods);


        var stringifyFunction = (CSharpMethodModel)stringSumFunction.LocalFunctions[1];
        Assert.Equal("Stringify", stringifyFunction.Name);
        Assert.Equal(2, stringifyFunction.LocalFunctions.Count);
        Assert.Equal(2, stringifyFunction.CalledMethods.Count);

        var stringifyFunctionCalledMethod1 = stringifyFunction.CalledMethods[0];
        Assert.Equal("StringifyNumber", stringifyFunctionCalledMethod1.Name);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod1.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod1.LocationClassName);
        Assert.Equal("Value", stringifyFunctionCalledMethod1.MethodDefinitionNames[0]);
        Assert.Equal("add", stringifyFunctionCalledMethod1.MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[2]);
        Assert.Equal("Stringify(int, int)", stringifyFunctionCalledMethod1.MethodDefinitionNames[3]);
        Assert.Equal(1, stringifyFunctionCalledMethod1.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunctionCalledMethod1.ParameterTypes[0].Type.Name);

        var stringifyFunctionCalledMethod2 = stringifyFunction.CalledMethods[1];
        Assert.Equal("Calculate", stringifyFunctionCalledMethod2.Name);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod2.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", stringifyFunctionCalledMethod2.LocationClassName);
        Assert.Equal("Value", stringifyFunctionCalledMethod2.MethodDefinitionNames[0]);
        Assert.Equal("add", stringifyFunctionCalledMethod2.MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[2]);
        Assert.Equal("Stringify(int, int)", stringifyFunctionCalledMethod2.MethodDefinitionNames[3]);
        Assert.Equal(2, stringifyFunctionCalledMethod2.ParameterTypes.Count);
        Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[0].Type.Name);
        Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[1].Type.Name);


        var calculateFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[0];
        Assert.Equal("Calculate", calculateFunction.Name);
        Assert.Empty(calculateFunction.LocalFunctions);
        Assert.Equal(1, calculateFunction.CalledMethods.Count);

        var calculateFunctionCalledMethod = calculateFunction.CalledMethods[0];
        Assert.Equal("Sum", calculateFunctionCalledMethod.Name);
        Assert.Equal("Namespace1.Class1", calculateFunctionCalledMethod.DefinitionClassName);
        Assert.Equal("Namespace1.Class1", calculateFunctionCalledMethod.LocationClassName);
        Assert.Equal("Value", calculateFunctionCalledMethod.MethodDefinitionNames[0]);
        Assert.Equal("add", calculateFunctionCalledMethod.MethodDefinitionNames[1]);
        Assert.Equal("StringSum(int, int)", calculateFunctionCalledMethod.MethodDefinitionNames[2]);
        Assert.Equal(2, calculateFunctionCalledMethod.ParameterTypes.Count);
        Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[0].Type.Name);
        Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[1].Type.Name);


        var stringifyNumberFunction = (CSharpMethodModel)stringifyFunction.LocalFunctions[1];
        Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
        Assert.Empty(stringifyNumberFunction.LocalFunctions);
        Assert.Equal(1, stringifyNumberFunction.CalledMethods.Count);

        var stringifyNumberFunctionCalledMethod = stringifyNumberFunction.CalledMethods[0];
        Assert.Equal("ToString", stringifyNumberFunctionCalledMethod.Name);
        Assert.Equal("int", stringifyNumberFunctionCalledMethod.DefinitionClassName);
        Assert.Equal("int", stringifyNumberFunctionCalledMethod.LocationClassName);
        Assert.Empty(stringifyNumberFunctionCalledMethod.MethodDefinitionNames);
        Assert.Empty(stringifyNumberFunctionCalledMethod.ParameterTypes);
    }
}
