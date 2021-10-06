﻿using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;

namespace Honeydew
{
    internal static class VisitorLoaderHelper
    {
        public static ICompositeVisitor LoadVisitors(IRelationMetricHolder relationMetricHolder, ILogger logger)
        {
            var linesOfCodeVisitor = new LinesOfCodeVisitor();

            var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            });

            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });

            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor(),
                attributeSetterVisitor
            });

            var genericParameterSetterVisitor = new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
            {
                new GenericParameterInfoVisitor(),
                attributeSetterVisitor
            });

            var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });

            var localFunctionsSetterClassVisitor = new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
            {
                new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                {
                    calledMethodSetterVisitor,
                    linesOfCodeVisitor,
                    parameterSetterVisitor,
                    localVariablesTypeSetterVisitor,
                    genericParameterSetterVisitor,
                }),
                calledMethodSetterVisitor,
                linesOfCodeVisitor,
                parameterSetterVisitor,
                localVariablesTypeSetterVisitor,
                genericParameterSetterVisitor,
            });

            var methodInfoVisitor = new MethodInfoVisitor();

            var methodVisitors = new List<ICSharpMethodVisitor>
            {
                methodInfoVisitor,
                linesOfCodeVisitor,
                calledMethodSetterVisitor,
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
                parameterSetterVisitor,
                localVariablesTypeSetterVisitor,
                genericParameterSetterVisitor,
            };

            var constructorVisitors = new List<ICSharpConstructorVisitor>
            {
                new ConstructorInfoVisitor(),
                linesOfCodeVisitor,
                calledMethodSetterVisitor,
                new ConstructorCallsVisitor(),
                localFunctionsSetterClassVisitor,
                attributeSetterVisitor,
                parameterSetterVisitor,
                localVariablesTypeSetterVisitor,
            };

            var fieldVisitors = new List<ICSharpFieldVisitor>
            {
                new FieldInfoVisitor(),
                attributeSetterVisitor,
            };

            var propertyVisitors = new List<ICSharpPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                {
                    methodInfoVisitor,
                    calledMethodSetterVisitor,
                    attributeSetterVisitor,
                    linesOfCodeVisitor,
                    localFunctionsSetterClassVisitor,
                    localVariablesTypeSetterVisitor,
                }),
                linesOfCodeVisitor,
                attributeSetterVisitor,
            };

            var classVisitors = new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new BaseTypesClassVisitor(),
                new MethodSetterClassVisitor(methodVisitors),
                new ConstructorSetterClassVisitor(constructorVisitors),
                new FieldSetterClassVisitor(fieldVisitors),
                new PropertySetterClassVisitor(propertyVisitors),
                new ImportsVisitor(),
                linesOfCodeVisitor,
                attributeSetterVisitor,
                genericParameterSetterVisitor,

                // metrics visitor
                new IsAbstractClassVisitor(),
                new ExceptionsThrownRelationVisitor(relationMetricHolder),
                new ObjectCreationRelationVisitor(relationMetricHolder),
            };

            var delegateVisitors = new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                new ImportsVisitor(),
                attributeSetterVisitor,
                parameterSetterVisitor,
                genericParameterSetterVisitor,
            };
            var compilationUnitVisitors = new List<ICSharpCompilationUnitVisitor>
            {
                new ClassSetterCompilationUnitVisitor(classVisitors),
                new DelegateSetterCompilationUnitVisitor(delegateVisitors),
                new ImportsVisitor(),
                linesOfCodeVisitor,
            };

            var compositeVisitor = new CompositeVisitor();

            foreach (var compilationUnitVisitor in compilationUnitVisitors)
            {
                compositeVisitor.Add(compilationUnitVisitor);
            }

            compositeVisitor.Accept(new LoggerSetterVisitor(logger));


            return compositeVisitor;
        }
    }
}
