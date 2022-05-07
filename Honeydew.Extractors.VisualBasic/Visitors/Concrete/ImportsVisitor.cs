using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ImportsVisitor :
    IExtractionVisitor<CompilationUnitSyntax, SemanticModel, ICompilationUnitType>,
    IExtractionVisitor<ClassBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<DelegateStatementSyntax, SemanticModel, IDelegateType>,
    IExtractionVisitor<EnumBlockSyntax, SemanticModel, IEnumType>
{
    public ICompilationUnitType Visit(CompilationUnitSyntax syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        var usingSet = new HashSet<string>();

        foreach (var importsStatementSyntax in syntaxNode.DescendantNodes().OfType<ImportsStatementSyntax>())
        {
            foreach (var importsClause in importsStatementSyntax.ImportsClauses)
            {
                switch (importsClause)
                {
                    case SimpleImportsClauseSyntax simpleImportsClauseSyntax:
                        var importName = simpleImportsClauseSyntax.Name.ToString();

                        if (usingSet.Contains(importName))
                        {
                            continue;
                        }

                        usingSet.Add(importName);

                        var importModel = CreateVisualBasicImportModel(simpleImportsClauseSyntax);

                        modelType.Imports.Add(importModel);
                        break;
                }
            }
        }

        return modelType;
    }

    public IMembersClassType Visit(ClassBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    public IMembersClassType Visit(InterfaceBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    public IMembersClassType Visit(StructureBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    public IDelegateType Visit(DelegateStatementSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    public IEnumType Visit(EnumBlockSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        foreach (var importType in ExtractParentImports(syntaxNode))
        {
            modelType.Imports.Add(importType);
        }

        return modelType;
    }

    private static List<IImportType> ExtractParentImports(SyntaxNode syntaxNode)
    {
        var usingSet = new HashSet<string>();
        var importTypes = new List<IImportType>();

        var parent = syntaxNode.Parent;

        while (parent != null)
        {
            foreach (var importsStatementSyntax in parent.ChildNodes().OfType<ImportsStatementSyntax>())
            {
                foreach (var importsClause in importsStatementSyntax.ImportsClauses)
                {
                    switch (importsClause)
                    {
                        case SimpleImportsClauseSyntax simpleImportsClauseSyntax:
                            var usingName = simpleImportsClauseSyntax.Name.ToString();

                            if (usingSet.Contains(usingName))
                            {
                                continue;
                            }

                            usingSet.Add(usingName);
                            var usingModel = CreateVisualBasicImportModel(simpleImportsClauseSyntax);
                            importTypes.Add(usingModel);
                            break;
                    }
                }
            }

            parent = parent.Parent;
        }

        return importTypes;
    }

    private static VisualBasicImportModel CreateVisualBasicImportModel(
        SimpleImportsClauseSyntax simpleImportsClauseSyntax)
    {
        var importModel = new VisualBasicImportModel
        {
            Name = simpleImportsClauseSyntax.Name.ToString(),
            AliasType = simpleImportsClauseSyntax.Alias is null
                ? nameof(EAliasType.None)
                : nameof(EAliasType.Namespace),
            Alias = simpleImportsClauseSyntax.Alias?.Identifier.ToString() ?? "",
            IsStatic = false
        };
        return importModel;
    }
}
