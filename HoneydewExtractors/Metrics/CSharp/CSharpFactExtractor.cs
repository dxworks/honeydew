using System;
using System.Collections.Generic;
using HoneydewExtractors.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp;
using HoneydewExtractors.Metrics.Extraction.CompilationUnitLevel;
using HoneydewExtractors.Metrics.Extraction.CompilationUnitLevel.CSharp;
using HoneydewModels;

namespace HoneydewExtractors.Metrics.CSharp
{
    public class
        CSharpFactExtractor : FactExtractor<ClassModel, CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpFactExtractor() : base(new CSharpSyntacticModelCreator(),
            new CSharpSemanticModelCreator(), new CSharpClassModelExtractor())
        {
        }

        protected override IDictionary<Type, Type> PopulateWithConcreteTypes()
        {
            return new Dictionary<Type, Type>
            {
                {typeof(IBaseClassMetric), typeof(CSharpBaseClassMetric)},
                {typeof(IFieldsInfoMetric), typeof(CSharpFieldsInfoMetric)},
                {typeof(IMethodInfoMetric), typeof(CSharpMethodInfoMetric)},
                {typeof(IIsAbstractMetric), typeof(CSharpIsAbstractMetric)},
                {typeof(ILocalVariablesDependencyMetric), typeof(CSharpLocalVariablesDependencyMetric)},
                {typeof(IParameterDependencyMetric), typeof(CSharpParameterDependencyMetric)},                
                {typeof(IReturnValueDependencyMetric), typeof(CSharpReturnValueDependencyMetric)},
                {typeof(IUsingsCountMetric), typeof(CSharpUsingsCountMetric)},
            };
        }
    }
}
