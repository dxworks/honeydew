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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Constructor.MethodCalls;

public class CSharpCalledMethodsInConstructorTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpCalledMethodsInConstructorTests()
    {
        var calledMethodSetterVisitor = new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor()
            });
        var parameterSetterVisitor = new CSharpParameterSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IParameterType>>
            {
                new ParameterInfoVisitor()
            });
        var returnValueSetterVisitor = new CSharpReturnValueSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IReturnValueType>>
            {
                new ReturnValueInfoVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                calledMethodSetterVisitor,
                                parameterSetterVisitor
                            }),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            parameterSetterVisitor,
                            returnValueSetterVisitor
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ConstructorCallsOtherMethods.txt")]
    public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorThatCallsOtherMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);
        Assert.Equal(2, classModel.Methods.Count);

        var functionMethod = classModel.Methods[0];
        Assert.Equal("Function", functionMethod.Name);
        Assert.Equal("void", functionMethod.ReturnValue.Type.Name);
        Assert.Equal(1, functionMethod.ParameterTypes.Count);
        var functionMethodParameter = (CSharpParameterModel)functionMethod.ParameterTypes[0];
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
        var computeMethodParameter = (CSharpParameterModel)computeMethod.ParameterTypes[0];
        Assert.Equal("int", computeMethodParameter.Type.Name);
        Assert.Equal("", computeMethodParameter.Modifier);
        Assert.Null(computeMethodParameter.DefaultValue);
        Assert.Equal("public", computeMethod.AccessModifier);
        Assert.Equal("", computeMethod.Modifier);
        Assert.Empty(computeMethod.CalledMethods);

        var intArgConstructor = classModel.Constructors[0];
        Assert.Equal("Foo", intArgConstructor.Name);
        Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
        var intArgConstructorParameter = (CSharpParameterModel)intArgConstructor.ParameterTypes[0];
        Assert.Equal("int", intArgConstructorParameter.Type.Name);
        Assert.Equal("", intArgConstructorParameter.Modifier);
        Assert.Null(intArgConstructorParameter.DefaultValue);
        Assert.Equal("public", intArgConstructor.AccessModifier);
        Assert.Equal("", intArgConstructor.Modifier);
        Assert.Equal(2, intArgConstructor.CalledMethods.Count);

        Assert.Equal("Function", intArgConstructor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].LocationClassName);
        Assert.Empty(intArgConstructor.CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);

        var methodSignatureType = intArgConstructor.CalledMethods[1];
        Assert.Equal("Compute", methodSignatureType.Name);
        Assert.Equal("TopLevel.Foo", methodSignatureType.DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodSignatureType.LocationClassName);
        Assert.Empty(methodSignatureType.MethodDefinitionNames);
        Assert.Equal(1, methodSignatureType.ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)methodSignatureType.ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
    }
}
