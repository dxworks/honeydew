using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;

namespace Honeydew.Extraction;

internal static class VisualBasicExtractionVisitors
{
    public static CompositeVisitor<ICompilationUnitType> GetVisitors(ILogger logger)
    {
        throw new NotImplementedException();
        // var linesOfCodeVisitor = new LinesOfCodeVisitor();
        //
        // var attributeSetterVisitor = new VisualBasicAttributeSetterVisitor(logger,
        //     new List<ITypeVisitor<IAttributeType>>
        //     {
        //         new AttributeInfoVisitor()
        //     });
        //
        // var calledMethodSetterVisitor = new VisualBasicCalledMethodSetterVisitor(logger,
        //     new List<ITypeVisitor<IMethodCallType>>
        //     {
        //         new MethodCallInfoVisitor()
        //     });
        //
        // var accessedFieldsSetterVisitor = new VisualBasicAccessedFieldsSetterVisitor(logger,
        //     new List<ITypeVisitor<AccessedField>>
        //     {
        //         new AccessFieldVisitor()
        //     });
        //
        // var parameterSetterVisitor = new VisualBasicParameterSetterVisitor(logger,
        //     new List<ITypeVisitor<IParameterType>>
        //     {
        //         new ParameterInfoVisitor(),
        //         attributeSetterVisitor
        //     });
        //
        // var returnValueSetterVisitor = new VisualBasicReturnValueSetterVisitor(logger,
        //     new List<ITypeVisitor<IReturnValueType>>
        //     {
        //         new ReturnValueInfoVisitor(),
        //         attributeSetterVisitor,
        //     });
        //
        // var genericParameterSetterVisitor = new VisualBasicGenericParameterSetterVisitor(logger,
        //     new List<ITypeVisitor<IGenericParameterType>>
        //     {
        //         new GenericParameterInfoVisitor(),
        //         attributeSetterVisitor
        //     });
        //
        // var localVariablesTypeSetterVisitor = new VisualBasicLocalVariablesTypeSetterVisitor(logger,
        //     new List<ITypeVisitor<ILocalVariableType>>
        //     {
        //         new LocalVariableInfoVisitor()
        //     });
        //
        // var gotoInfoVisitor = new GotoStatementVisitor();
        //
        // var localFunctionsSetterClassVisitor = new VisualBasicLocalFunctionsSetterClassVisitor(logger,
        //     new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
        //     {
        //         new LocalFunctionInfoVisitor(logger, new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
        //         {
        //             calledMethodSetterVisitor,
        //             linesOfCodeVisitor,
        //             parameterSetterVisitor,
        //             returnValueSetterVisitor,
        //             localVariablesTypeSetterVisitor,
        //             genericParameterSetterVisitor,
        //             accessedFieldsSetterVisitor,
        //             gotoInfoVisitor,
        //         }),
        //         calledMethodSetterVisitor,
        //         linesOfCodeVisitor,
        //         parameterSetterVisitor,
        //         returnValueSetterVisitor,
        //         localVariablesTypeSetterVisitor,
        //         genericParameterSetterVisitor,
        //         accessedFieldsSetterVisitor,
        //         gotoInfoVisitor,
        //     });
        //
        // var methodInfoVisitor = new MethodInfoVisitor();
        //
        // var methodVisitors = new List<ITypeVisitor<IMethodType>>
        // {
        //     methodInfoVisitor,
        //     linesOfCodeVisitor,
        //     calledMethodSetterVisitor,
        //     localFunctionsSetterClassVisitor,
        //     attributeSetterVisitor,
        //     parameterSetterVisitor,
        //     returnValueSetterVisitor,
        //     localVariablesTypeSetterVisitor,
        //     genericParameterSetterVisitor,
        //     accessedFieldsSetterVisitor,
        //     gotoInfoVisitor,
        // };
        //
        // var constructorVisitors = new List<ITypeVisitor<IConstructorType>>
        // {
        //     new ConstructorInfoVisitor(),
        //     linesOfCodeVisitor,
        //     calledMethodSetterVisitor,
        //     new ConstructorCallsVisitor(),
        //     localFunctionsSetterClassVisitor,
        //     attributeSetterVisitor,
        //     parameterSetterVisitor,
        //     localVariablesTypeSetterVisitor,
        //     accessedFieldsSetterVisitor,
        //     gotoInfoVisitor,
        // };
        //
        // var destructorVisitors = new List<ITypeVisitor<IDestructorType>>
        // {
        //     new DestructorInfoVisitor(),
        //     linesOfCodeVisitor,
        //     calledMethodSetterVisitor,
        //     localFunctionsSetterClassVisitor,
        //     attributeSetterVisitor,
        //     localVariablesTypeSetterVisitor,
        //     accessedFieldsSetterVisitor,
        //     gotoInfoVisitor,
        // };
        //
        // var fieldVisitors = new List<ITypeVisitor<IFieldType>>
        // {
        //     new FieldInfoVisitor(),
        //     attributeSetterVisitor,
        // };
        //
        // var propertyAccessorsVisitors = new List<ITypeVisitor<IAccessorMethodType>>
        // {
        //     methodInfoVisitor,
        //     calledMethodSetterVisitor,
        //     attributeSetterVisitor,
        //     linesOfCodeVisitor,
        //     returnValueSetterVisitor,
        //     localFunctionsSetterClassVisitor,
        //     localVariablesTypeSetterVisitor,
        //     accessedFieldsSetterVisitor,
        //     gotoInfoVisitor,
        // };
        //
        // var propertyVisitors = new List<ITypeVisitor<IPropertyType>>
        // {
        //     new PropertyInfoVisitor(),
        //     new VisualBasicAccessorMethodSetterPropertyVisitor(logger, propertyAccessorsVisitors),
        //     linesOfCodeVisitor,
        //     attributeSetterVisitor,
        // };
        //
        // var importsVisitor = new ImportsVisitor();
        //
        // var classVisitors = new List<ITypeVisitor<IMembersClassType>>
        // {
        //     new BaseInfoClassVisitor(),
        //     new BaseTypesClassVisitor(),
        //     new VisualBasicMethodSetterClassVisitor(logger, methodVisitors),
        //     new VisualBasicConstructorSetterClassVisitor(logger, constructorVisitors),
        //     new VisualBasicDestructorSetterClassVisitor(logger, destructorVisitors),
        //     new VisualBasicFieldSetterClassVisitor(logger, fieldVisitors),
        //     new VisualBasicPropertySetterClassVisitor(logger, propertyVisitors),
        //     importsVisitor,
        //     linesOfCodeVisitor,
        //     attributeSetterVisitor,
        //     genericParameterSetterVisitor,
        //
        //     // metrics visitor
        //     new ExceptionsThrownRelationVisitor(),
        //     new ObjectCreationRelationVisitor(logger),
        // };
        //
        // var delegateVisitors = new List<ITypeVisitor<IDelegateType>>
        // {
        //     new BaseInfoDelegateVisitor(),
        //     importsVisitor,
        //     attributeSetterVisitor,
        //     parameterSetterVisitor,
        //     returnValueSetterVisitor,
        //     genericParameterSetterVisitor,
        //     linesOfCodeVisitor,
        // };
        //
        // var enumVisitors = new List<ITypeVisitor<IEnumType>>
        // {
        //     new BaseInfoEnumVisitor(),
        //     new VisualBasicEnumLabelsSetterVisitor(logger, new List<ITypeVisitor<IEnumLabelType>>
        //     {
        //         new BasicEnumLabelInfoVisitor(),
        //         attributeSetterVisitor,
        //     }),
        //     importsVisitor,
        //     attributeSetterVisitor,
        //     linesOfCodeVisitor,
        // };
        //
        // var compilationUnitVisitors = new List<ITypeVisitor<ICompilationUnitType>>
        // {
        //     new VisualBasicClassSetterVisitor(logger, classVisitors),
        //     new VisualBasicDelegateSetterCompilationUnitVisitor(logger, delegateVisitors),
        //     new VisualBasicEnumSetterCompilationUnitVisitor(logger, enumVisitors),
        //     importsVisitor,
        //     linesOfCodeVisitor,
        // };
        //
        // return new VisualBasicCompilationUnitCompositeVisitor(logger, compilationUnitVisitors);
    }
}
