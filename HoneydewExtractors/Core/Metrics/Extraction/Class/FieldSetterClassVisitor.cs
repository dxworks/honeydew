using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class FieldSetterClassVisitor : CompositeTypeVisitor, ICSharpClassVisitor
    {
        private readonly CSharpFieldModelCreator _cSharpFieldModelCreator;

        public FieldSetterClassVisitor(CSharpFieldModelCreator cSharpFieldModelCreator)
        {
            _cSharpFieldModelCreator = cSharpFieldModelCreator;

            foreach (var visitor in _cSharpFieldModelCreator.GetVisitors())
            {
                Add(visitor);
            }
        }

        public IMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IMembersClassType modelType)
        {
            foreach (var baseFieldDeclarationSyntax in
                syntaxNode.DescendantNodes().OfType<BaseFieldDeclarationSyntax>())
            {
                foreach (var fieldType in _cSharpFieldModelCreator.Create(baseFieldDeclarationSyntax,
                    new List<IFieldType>()))
                {
                    modelType.Fields.Add(fieldType);
                }
            }

            return modelType;
        }
    }
}
