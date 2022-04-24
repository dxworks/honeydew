using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.LocalFunction.AccessedFields;

public class LocalFunctionsAccessedFieldsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public LocalFunctionsAccessedFieldsTests()
    {
        var accessedFieldsSetterVisitor = new CSharpAccessedFieldsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<AccessedField>>
            {
                new AccessFieldVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            new CSharpLocalFunctionsSetterClassVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                                {
                                    new LocalFunctionInfoVisitor(_loggerMock.Object,
                                        new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                                        {
                                            accessedFieldsSetterVisitor
                                        }),
                                    accessedFieldsSetterVisitor
                                }),
                        })
                    })
            });


        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/LocalFunctionsAccessedNonStaticFieldAndPropertyFromClass.txt")]
    [FileData("TestData/LocalFunctionsAccessedStaticFieldAndPropertyFromClass.txt")]
    public void
        Extract_ShouldHaveAccessedFields_WhenGivenPropertyAccessorThatAccessesFieldsAndPropertiesFromOtherClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var modelMethod1 = (MethodModel)classModel.Methods[0];
        var modelMethod2 = (MethodModel)classModel.Methods[1];

        var localFunctions = new[]
        {
            modelMethod1.LocalFunctions[0],
            modelMethod1.LocalFunctions[0].LocalFunctions[0],
            modelMethod1.LocalFunctions[0].LocalFunctions[0].LocalFunctions[0],

            modelMethod2.LocalFunctions[0],
            modelMethod2.LocalFunctions[0].LocalFunctions[0],
            modelMethod2.LocalFunctions[0].LocalFunctions[0].LocalFunctions[0],
        };

        foreach (var localFunction in localFunctions)
        {
            Assert.Equal(2, localFunction.AccessedFields.Count);

            Assert.Equal("Field1", localFunction.AccessedFields[0].Name);
            Assert.Equal("Property1", localFunction.AccessedFields[1].Name);

            foreach (var accessedField in localFunction.AccessedFields)
            {
                Assert.Equal(classModel.Name, accessedField.DefinitionClassName);
                Assert.Equal(classModel.Name, accessedField.LocationClassName);
            }
        }

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(AccessedField.AccessKind.Getter, localFunctions[i].AccessedFields[0].Kind);
            Assert.Equal(AccessedField.AccessKind.Getter, localFunctions[i].AccessedFields[1].Kind);
        }

        for (var i = 3; i < localFunctions.Length; i++)
        {
            Assert.Equal(AccessedField.AccessKind.Setter, localFunctions[i].AccessedFields[0].Kind);
            Assert.Equal(AccessedField.AccessKind.Setter, localFunctions[i].AccessedFields[1].Kind);
        }
    }
}
