using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Constructor.MethodCalls;

public class CSharpCalledMethodsInConstructor
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpCalledMethodsInConstructor()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        var calledMethodSetterVisitor = new CalledMethodSetterVisitor(_loggerMock.Object,
            new List<ICSharpMethodCallVisitor>
            {
                new MethodCallInfoVisitor()
            });
        var parameterSetterVisitor = new ParameterSetterVisitor(_loggerMock.Object, new List<IParameterVisitor>
        {
            new ParameterInfoVisitor()
        });
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new ConstructorSetterClassVisitor(_loggerMock.Object, new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                calledMethodSetterVisitor,
                parameterSetterVisitor
            }),
            new MethodSetterClassVisitor(_loggerMock.Object, new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
                parameterSetterVisitor,
            })
        }));

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
        Assert.Empty(intArgConstructor.CalledMethods[0].MethodDefinitionNames);
        Assert.Equal(1, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (ParameterModel)intArgConstructor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);

        var methodSignatureType = intArgConstructor.CalledMethods[1];
        Assert.Equal("Compute", methodSignatureType.Name);
        Assert.Equal("TopLevel.Foo", methodSignatureType.DefinitionClassName);
        Assert.Equal("TopLevel.Foo", methodSignatureType.LocationClassName);
        Assert.Empty(methodSignatureType.MethodDefinitionNames);
        Assert.Equal(1, methodSignatureType.ParameterTypes.Count);
        var parameterModel2 = (ParameterModel)methodSignatureType.ParameterTypes[0];
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
    }
}
