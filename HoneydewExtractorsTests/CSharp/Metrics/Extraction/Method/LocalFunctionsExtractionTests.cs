using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Constructor;
using HoneydewExtractors.Core.Metrics.Extraction.Method;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Parameter;
using HoneydewExtractors.Core.Metrics.Extraction.Property;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class LocalFunctionsExtractionTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public LocalFunctionsExtractionTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor(),
            });
            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor()
            });
            var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(
                new List<ICSharpLocalFunctionVisitor>
                {
                    calledMethodSetterVisitor,
                    new LocalFunctionInfoVisitor(new List<ICSharpLocalFunctionVisitor>
                    {
                        calledMethodSetterVisitor,
                        parameterSetterVisitor
                    }),
                    parameterSetterVisitor
                });

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor,
                    localFunctionsSetterClassVisitor,
                }),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    localFunctionsSetterClassVisitor
                }),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    localFunctionsSetterClassVisitor
                })
            }));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithVoidLocalFunctionWithNoParameters.txt")]
        public void
            Extract_ShouldExtractLocalFunction_WhenGivenMethodWithOneLocalFunctionThatReturnsVoidAndHasNoParameters(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(1, method.LocalFunctions.Count);
            Assert.Equal("LocalFunction", method.LocalFunctions[0].Name);
            Assert.Equal("", method.LocalFunctions[0].Modifier);
            Assert.Equal("", method.LocalFunctions[0].AccessModifier);
            Assert.Equal("Namespace1.Class1.Method()", method.LocalFunctions[0].ContainingTypeName);
            Assert.Empty(method.LocalFunctions[0].ParameterTypes);
            Assert.Equal("void", method.LocalFunctions[0].ReturnValue.Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithLocalFunctionWithReturnValuesAndParameters.txt")]
        public void Extract_ShouldExtractLocalFunction_WhenGivenLocalFunctionWithReturnValueAndParameters(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(1, method.LocalFunctions.Count);
            Assert.Equal("Sum", method.LocalFunctions[0].Name);
            Assert.Equal("", method.LocalFunctions[0].Modifier);
            Assert.Equal("", method.LocalFunctions[0].AccessModifier);
            Assert.Equal("Namespace1.Class1.Method(int, int)", method.LocalFunctions[0].ContainingTypeName);
            Assert.Equal(2, method.LocalFunctions[0].ParameterTypes.Count);
            Assert.Equal("int", method.LocalFunctions[0].ParameterTypes[0].Type.Name);
            Assert.Equal("int", method.LocalFunctions[0].ParameterTypes[1].Type.Name);
            Assert.Equal("int", method.LocalFunctions[0].ReturnValue.Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithMultipleLocalFunctionsWithModifiers.txt")]
        public void Extract_ShouldExtractMultipleLocalFunctions_WhenGivenLocalFunctionsWithModifiers(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(3, method.LocalFunctions.Count);

            var localFunction = method.LocalFunctions[0];
            Assert.Equal("Function", localFunction.Name);
            Assert.Equal("async", localFunction.Modifier);
            Assert.Equal("Namespace1.Class1.Method(int, int)", localFunction.ContainingTypeName);
            Assert.Equal(1, localFunction.ParameterTypes.Count);
            Assert.Equal("int", localFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Threading.Tasks.Task<int>", localFunction.ReturnValue.Type.Name);

            var staticFunction = method.LocalFunctions[1];
            Assert.Equal("StaticFunction", staticFunction.Name);
            Assert.Equal("static", staticFunction.Modifier);
            Assert.Equal("Namespace1.Class1.Method(int, int)", staticFunction.ContainingTypeName);
            Assert.Empty(staticFunction.ParameterTypes);
            Assert.Equal("System.Threading.Tasks.Task<int>", staticFunction.ReturnValue.Type.Name);

            var externFunction = method.LocalFunctions[2];
            Assert.Equal("ExternFunction", externFunction.Name);
            Assert.Equal("static extern", externFunction.Modifier);
            Assert.Equal("Namespace1.Class1.Method(int, int)", externFunction.ContainingTypeName);
            Assert.Equal(1, externFunction.ParameterTypes.Count);
            Assert.Equal("string", externFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", externFunction.ReturnValue.Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithLocalFunction_CyclomaticComplexity.txt")]
        public void
            Extract_ShouldExtractLocalFunctionCyclomaticComplexity_WhenGivenLocalFunctionWithHighCyclomaticComplexity(
                string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(1, method.LocalFunctions.Count);
            Assert.Equal("Function", method.LocalFunctions[0].Name);
            Assert.Equal(11, method.LocalFunctions[0].CyclomaticComplexity);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodPropertyConstructorWithLocalFunction.txt")]
        public void Extract_ShouldExtractLocalFunction_WhenGivenMethodPropertyConstructorWithLocalFunction(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];
            var constructor = (ConstructorModel)((ClassModel)classTypes[0]).Constructors[0];
            var property = (PropertyModel)((ClassModel)classTypes[0]).Properties[0];
            var eventProperty = (PropertyModel)((ClassModel)classTypes[0]).Properties[1];

            var localFunctions = new[]
            {
                method.LocalFunctions[0],
                constructor.LocalFunctions[0],
                property.LocalFunctions[0],
                eventProperty.LocalFunctions[0],
                eventProperty.LocalFunctions[1],
            };

            foreach (var localFunction in localFunctions)
            {
                Assert.Equal("Function", localFunction.Name);
                Assert.Equal("static", localFunction.Modifier);
                Assert.Equal(1, localFunction.ParameterTypes.Count);
                Assert.Equal("int", localFunction.ParameterTypes[0].Type.Name);
                Assert.Equal("string", localFunction.ReturnValue.Type.Name);
            }

            Assert.Equal("Namespace1.Class1.Method(int)", method.LocalFunctions[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Class1(int)", constructor.LocalFunctions[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Value", property.LocalFunctions[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.EventValue", eventProperty.LocalFunctions[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.EventValue", eventProperty.LocalFunctions[1].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithMultipleLocalFunctions_ThatAreCalled.txt")]
        public void Extract_ShouldExtractLocalFunctionAndUsages_WhenGivenMethodWithMultipleLocalFunctions(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(4, method.LocalFunctions.Count);

            foreach (var function in method.LocalFunctions)
            {
                Assert.Equal("Namespace1.Class1.Method(int, int, int)", function.ContainingTypeName);
                Assert.Equal("", function.Modifier);
                Assert.Equal("", function.AccessModifier);

                foreach (var calledMethod in function.CalledMethods)
                {
                    Assert.Equal("Namespace1.Class1.Method(int, int, int)", calledMethod.ContainingTypeName);
                }
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
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithImbricatedLocalFunctions.txt")]
        public void Extract_ShouldExtractLocalFunctions_WhenGivenMethodWithImbricatedLocalFunctions(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(1, method.LocalFunctions.Count);

            var stringSumFunction = (MethodModel)method.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Method(int, int)", stringSumFunction.ContainingTypeName);
            Assert.Equal("StringSum", stringSumFunction.Name);
            Assert.Equal("string", stringSumFunction.ReturnValue.Type.Name);
            Assert.Equal(2, stringSumFunction.ParameterTypes.Count);
            Assert.Equal("int", stringSumFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringSumFunction.ParameterTypes[1].Type.Name);
            Assert.Equal(2, stringSumFunction.LocalFunctions.Count);


            var sumFunction = (MethodModel)stringSumFunction.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int)", sumFunction.ContainingTypeName);
            Assert.Equal("Sum", sumFunction.Name);
            Assert.Equal("int", sumFunction.ReturnValue.Type.Name);
            Assert.Equal(2, sumFunction.ParameterTypes.Count);
            Assert.Equal("int", sumFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", sumFunction.ParameterTypes[1].Type.Name);
            Assert.Equal(1, sumFunction.LocalFunctions.Count);

            var doubledFunction = (MethodModel)sumFunction.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int).Sum(int, int)",
                doubledFunction.ContainingTypeName);
            Assert.Equal("Doubled", doubledFunction.Name);
            Assert.Equal("int", doubledFunction.ReturnValue.Type.Name);
            Assert.Equal(1, doubledFunction.ParameterTypes.Count);
            Assert.Equal("int", doubledFunction.ParameterTypes[0].Type.Name);
            Assert.Empty(doubledFunction.LocalFunctions);


            var stringifyFunction = (MethodModel)stringSumFunction.LocalFunctions[1];
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int)",
                stringifyFunction.ContainingTypeName);
            Assert.Equal("Stringify", stringifyFunction.Name);
            Assert.Equal("string", stringifyFunction.ReturnValue.Type.Name);
            Assert.Equal(2, stringifyFunction.ParameterTypes.Count);
            Assert.Equal("int", stringifyFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringifyFunction.ParameterTypes[1].Type.Name);
            Assert.Equal(2, stringifyFunction.LocalFunctions.Count);

            var calculateFunction = (MethodModel)stringifyFunction.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int).Stringify(int, int)",
                calculateFunction.ContainingTypeName);
            Assert.Equal("Calculate", calculateFunction.Name);
            Assert.Equal("int", calculateFunction.ReturnValue.Type.Name);
            Assert.Equal(2, calculateFunction.ParameterTypes.Count);
            Assert.Equal("int", calculateFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", calculateFunction.ParameterTypes[1].Type.Name);
            Assert.Empty(calculateFunction.LocalFunctions);

            var stringifyNumberFunction = (MethodModel)stringifyFunction.LocalFunctions[1];
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int).Stringify(int, int)",
                stringifyNumberFunction.ContainingTypeName);
            Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
            Assert.Equal("string", stringifyNumberFunction.ReturnValue.Type.Name);
            Assert.Equal(1, stringifyNumberFunction.ParameterTypes.Count);
            Assert.Equal("int", stringifyNumberFunction.ParameterTypes[0].Type.Name);
            Assert.Empty(stringifyNumberFunction.LocalFunctions);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/MethodWithImbricatedLocalFunctions.txt")]
        public void Extract_ShouldExtractLocalFunctionsWithCalledMethods_WhenGivenMethodWithImbricatedLocalFunctions(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var method = (MethodModel)((ClassModel)classTypes[0]).Methods[0];

            Assert.Equal("Method", method.Name);
            Assert.Equal(1, method.LocalFunctions.Count);

            var stringSumFunction = (MethodModel)method.LocalFunctions[0];
            Assert.Equal("StringSum", stringSumFunction.Name);
            Assert.Equal(2, stringSumFunction.LocalFunctions.Count);
            Assert.Equal(1, stringSumFunction.CalledMethods.Count);
            Assert.Equal("Stringify", stringSumFunction.CalledMethods[0].Name);
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int)",
                stringSumFunction.CalledMethods[0].ContainingTypeName);
            Assert.Equal(2, stringSumFunction.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[1].Type.Name);

            var sumFunction = (MethodModel)stringSumFunction.LocalFunctions[0];
            Assert.Equal("Sum", sumFunction.Name);
            Assert.Equal(1, sumFunction.LocalFunctions.Count);
            Assert.Equal(2, sumFunction.CalledMethods.Count);
            foreach (var calledMethod in sumFunction.CalledMethods)
            {
                Assert.Equal("Doubled", calledMethod.Name);
                Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int).Sum(int, int)",
                    calledMethod.ContainingTypeName);
                Assert.Equal(1, calledMethod.ParameterTypes.Count);
                Assert.Equal("int", calledMethod.ParameterTypes[0].Type.Name);
            }

            var doubledFunction = (MethodModel)sumFunction.LocalFunctions[0];
            Assert.Equal("Doubled", doubledFunction.Name);
            Assert.Empty(doubledFunction.LocalFunctions);
            Assert.Empty(doubledFunction.CalledMethods);


            var stringifyFunction = (MethodModel)stringSumFunction.LocalFunctions[1];
            Assert.Equal("Stringify", stringifyFunction.Name);
            Assert.Equal(2, stringifyFunction.LocalFunctions.Count);
            Assert.Equal(2, stringifyFunction.CalledMethods.Count);

            var stringifyFunctionCalledMethod1 = stringifyFunction.CalledMethods[0];
            Assert.Equal("StringifyNumber", stringifyFunctionCalledMethod1.Name);
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int).Stringify(int, int)",
                stringifyFunctionCalledMethod1.ContainingTypeName);
            Assert.Equal(1, stringifyFunctionCalledMethod1.ParameterTypes.Count);
            Assert.Equal("int", stringifyFunctionCalledMethod1.ParameterTypes[0].Type.Name);

            var stringifyFunctionCalledMethod2 = stringifyFunction.CalledMethods[1];
            Assert.Equal("Calculate", stringifyFunctionCalledMethod2.Name);
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int).Stringify(int, int)",
                stringifyFunctionCalledMethod2.ContainingTypeName);
            Assert.Equal(2, stringifyFunctionCalledMethod2.ParameterTypes.Count);
            Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[1].Type.Name);


            var calculateFunction = (MethodModel)stringifyFunction.LocalFunctions[0];
            Assert.Equal("Calculate", calculateFunction.Name);
            Assert.Empty(calculateFunction.LocalFunctions);
            Assert.Equal(1, calculateFunction.CalledMethods.Count);

            var calculateFunctionCalledMethod = calculateFunction.CalledMethods[0];
            Assert.Equal("Sum", calculateFunctionCalledMethod.Name);
            Assert.Equal("Namespace1.Class1.Method(int, int).StringSum(int, int)",
                calculateFunctionCalledMethod.ContainingTypeName);
            Assert.Equal(2, calculateFunctionCalledMethod.ParameterTypes.Count);
            Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[0].Type.Name);
            Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[1].Type.Name);


            var stringifyNumberFunction = (MethodModel)stringifyFunction.LocalFunctions[1];
            Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
            Assert.Empty(stringifyNumberFunction.LocalFunctions);
            Assert.Equal(1, stringifyNumberFunction.CalledMethods.Count);

            var stringifyNumberFunctionCalledMethod = stringifyNumberFunction.CalledMethods[0];
            Assert.Equal("ToString", stringifyNumberFunctionCalledMethod.Name);
            Assert.Equal("int", stringifyNumberFunctionCalledMethod.ContainingTypeName);
            Assert.Empty(stringifyNumberFunctionCalledMethod.ParameterTypes);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/PropertyWithImbricatedLocalFunctions.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/EventPropertyWithImbricatedLocalFunctions.txt")]
        public void Extract_ShouldExtractLocalFunctions_WhenGivenPropertyWithImbricatedLocalFunctions(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var property = (PropertyModel)((ClassModel)classTypes[0]).Properties[0];

            Assert.Equal("Value", property.Name);
            Assert.Equal(1, property.LocalFunctions.Count);

            var stringSumFunction = (MethodModel)property.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Value", stringSumFunction.ContainingTypeName);
            Assert.Equal("StringSum", stringSumFunction.Name);
            Assert.Equal("string", stringSumFunction.ReturnValue.Type.Name);
            Assert.Equal(2, stringSumFunction.ParameterTypes.Count);
            Assert.Equal("int", stringSumFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringSumFunction.ParameterTypes[1].Type.Name);
            Assert.Equal(2, stringSumFunction.LocalFunctions.Count);


            var sumFunction = (MethodModel)stringSumFunction.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int)", sumFunction.ContainingTypeName);
            Assert.Equal("Sum", sumFunction.Name);
            Assert.Equal("int", sumFunction.ReturnValue.Type.Name);
            Assert.Equal(2, sumFunction.ParameterTypes.Count);
            Assert.Equal("int", sumFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", sumFunction.ParameterTypes[1].Type.Name);
            Assert.Equal(1, sumFunction.LocalFunctions.Count);

            var doubledFunction = (MethodModel)sumFunction.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int).Sum(int, int)",
                doubledFunction.ContainingTypeName);
            Assert.Equal("Doubled", doubledFunction.Name);
            Assert.Equal("int", doubledFunction.ReturnValue.Type.Name);
            Assert.Equal(1, doubledFunction.ParameterTypes.Count);
            Assert.Equal("int", doubledFunction.ParameterTypes[0].Type.Name);
            Assert.Empty(doubledFunction.LocalFunctions);


            var stringifyFunction = (MethodModel)stringSumFunction.LocalFunctions[1];
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int)",
                stringifyFunction.ContainingTypeName);
            Assert.Equal("Stringify", stringifyFunction.Name);
            Assert.Equal("string", stringifyFunction.ReturnValue.Type.Name);
            Assert.Equal(2, stringifyFunction.ParameterTypes.Count);
            Assert.Equal("int", stringifyFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringifyFunction.ParameterTypes[1].Type.Name);
            Assert.Equal(2, stringifyFunction.LocalFunctions.Count);

            var calculateFunction = (MethodModel)stringifyFunction.LocalFunctions[0];
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int).Stringify(int, int)",
                calculateFunction.ContainingTypeName);
            Assert.Equal("Calculate", calculateFunction.Name);
            Assert.Equal("int", calculateFunction.ReturnValue.Type.Name);
            Assert.Equal(2, calculateFunction.ParameterTypes.Count);
            Assert.Equal("int", calculateFunction.ParameterTypes[0].Type.Name);
            Assert.Equal("int", calculateFunction.ParameterTypes[1].Type.Name);
            Assert.Empty(calculateFunction.LocalFunctions);

            var stringifyNumberFunction = (MethodModel)stringifyFunction.LocalFunctions[1];
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int).Stringify(int, int)",
                stringifyNumberFunction.ContainingTypeName);
            Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
            Assert.Equal("string", stringifyNumberFunction.ReturnValue.Type.Name);
            Assert.Equal(1, stringifyNumberFunction.ParameterTypes.Count);
            Assert.Equal("int", stringifyNumberFunction.ParameterTypes[0].Type.Name);
            Assert.Empty(stringifyNumberFunction.LocalFunctions);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/PropertyWithImbricatedLocalFunctions.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/EventPropertyWithImbricatedLocalFunctions.txt")]
        public void Extract_ShouldExtractLocalFunctionsWithCalledMethods_WhenGivenPropertyWithImbricatedLocalFunctions(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var property = (PropertyModel)((ClassModel)classTypes[0]).Properties[0];

            Assert.Equal("Value", property.Name);
            Assert.Equal(1, property.LocalFunctions.Count);

            var stringSumFunction = (MethodModel)property.LocalFunctions[0];
            Assert.Equal("StringSum", stringSumFunction.Name);
            Assert.Equal(2, stringSumFunction.LocalFunctions.Count);
            Assert.Equal(1, stringSumFunction.CalledMethods.Count);
            Assert.Equal("Stringify", stringSumFunction.CalledMethods[0].Name);
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int)",
                stringSumFunction.CalledMethods[0].ContainingTypeName);
            Assert.Equal(2, stringSumFunction.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringSumFunction.CalledMethods[0].ParameterTypes[1].Type.Name);

            var sumFunction = (MethodModel)stringSumFunction.LocalFunctions[0];
            Assert.Equal("Sum", sumFunction.Name);
            Assert.Equal(1, sumFunction.LocalFunctions.Count);
            Assert.Equal(2, sumFunction.CalledMethods.Count);
            foreach (var calledMethod in sumFunction.CalledMethods)
            {
                Assert.Equal("Doubled", calledMethod.Name);
                Assert.Equal("Namespace1.Class1.Value.StringSum(int, int).Sum(int, int)",
                    calledMethod.ContainingTypeName);
                Assert.Equal(1, calledMethod.ParameterTypes.Count);
                Assert.Equal("int", calledMethod.ParameterTypes[0].Type.Name);
            }

            var doubledFunction = (MethodModel)sumFunction.LocalFunctions[0];
            Assert.Equal("Doubled", doubledFunction.Name);
            Assert.Empty(doubledFunction.LocalFunctions);
            Assert.Empty(doubledFunction.CalledMethods);


            var stringifyFunction = (MethodModel)stringSumFunction.LocalFunctions[1];
            Assert.Equal("Stringify", stringifyFunction.Name);
            Assert.Equal(2, stringifyFunction.LocalFunctions.Count);
            Assert.Equal(2, stringifyFunction.CalledMethods.Count);

            var stringifyFunctionCalledMethod1 = stringifyFunction.CalledMethods[0];
            Assert.Equal("StringifyNumber", stringifyFunctionCalledMethod1.Name);
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int).Stringify(int, int)",
                stringifyFunctionCalledMethod1.ContainingTypeName);
            Assert.Equal(1, stringifyFunctionCalledMethod1.ParameterTypes.Count);
            Assert.Equal("int", stringifyFunctionCalledMethod1.ParameterTypes[0].Type.Name);

            var stringifyFunctionCalledMethod2 = stringifyFunction.CalledMethods[1];
            Assert.Equal("Calculate", stringifyFunctionCalledMethod2.Name);
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int).Stringify(int, int)",
                stringifyFunctionCalledMethod2.ContainingTypeName);
            Assert.Equal(2, stringifyFunctionCalledMethod2.ParameterTypes.Count);
            Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[0].Type.Name);
            Assert.Equal("int", stringifyFunctionCalledMethod2.ParameterTypes[1].Type.Name);


            var calculateFunction = (MethodModel)stringifyFunction.LocalFunctions[0];
            Assert.Equal("Calculate", calculateFunction.Name);
            Assert.Empty(calculateFunction.LocalFunctions);
            Assert.Equal(1, calculateFunction.CalledMethods.Count);

            var calculateFunctionCalledMethod = calculateFunction.CalledMethods[0];
            Assert.Equal("Sum", calculateFunctionCalledMethod.Name);
            Assert.Equal("Namespace1.Class1.Value.StringSum(int, int)",
                calculateFunctionCalledMethod.ContainingTypeName);
            Assert.Equal(2, calculateFunctionCalledMethod.ParameterTypes.Count);
            Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[0].Type.Name);
            Assert.Equal("int", calculateFunctionCalledMethod.ParameterTypes[1].Type.Name);


            var stringifyNumberFunction = (MethodModel)stringifyFunction.LocalFunctions[1];
            Assert.Equal("StringifyNumber", stringifyNumberFunction.Name);
            Assert.Empty(stringifyNumberFunction.LocalFunctions);
            Assert.Equal(1, stringifyNumberFunction.CalledMethods.Count);

            var stringifyNumberFunctionCalledMethod = stringifyNumberFunction.CalledMethods[0];
            Assert.Equal("ToString", stringifyNumberFunctionCalledMethod.Name);
            Assert.Equal("int", stringifyNumberFunctionCalledMethod.ContainingTypeName);
            Assert.Empty(stringifyNumberFunctionCalledMethod.ParameterTypes);
        }
    }
}
