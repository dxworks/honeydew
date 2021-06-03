using System.Linq;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors
{
    public class ClassExtractor : IExtractor
    {
        public ProjectEntity Extract(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new EmptyContentException();
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetCompilationUnitRoot();

            var diagnostics = root.GetDiagnostics();

            if (diagnostics.Any())
            {
                throw new ExtractionException();
            }

            var namespaceDeclarationSyntax = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
            var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var projectClass = new ProjectClass
            {
                Namespace = namespaceDeclarationSyntax.Name.ToString(),
                Name = classDeclarationSyntax.Identifier.ToString()
            };
            
            return projectClass;
        }
    }
}