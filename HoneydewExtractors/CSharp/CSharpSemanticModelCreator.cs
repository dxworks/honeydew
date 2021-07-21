using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.CSharp
{
    public class CSharpSemanticModelCreator : ISemanticModelCreator
    {
        public ISemanticModel Create(ISyntacticModel syntacticModel)
        {
            var cSharpSyntacticModel = syntacticModel as CSharpSyntacticModel;

            var semanticModel = CreateSemanticModel(cSharpSyntacticModel!.Tree);
            return new CSharpSemanticModel
            {
                Model = semanticModel
            };
        }
        
        private static SemanticModel CreateSemanticModel(SyntaxTree tree)
        {
            var compilation = CSharpCompilation.Create("Compilation");

            // try to add a reference to the System assembly
            var systemReference = typeof(object).Assembly.Location;

            // if 'systemReference' is empty means that the build is a single-file app and should look in the dlls to search for the System.dll 
            if (string.IsNullOrEmpty(systemReference))
            {
                var value = (string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
                if (value != null)
                {
                    var pathToDlls = value.Split(Path.PathSeparator);
                    var pathToSystem = pathToDlls.FirstOrDefault(path => path.Contains("System.dll"));

                    if (!string.IsNullOrEmpty(pathToSystem))
                    {
                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(pathToSystem));
                    }
                }
            }
            // if 'systemReference' is empty means that the System.dll Location is accessible with Reflection
            else
            {
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(systemReference));
            }

            compilation = compilation.AddSyntaxTrees(tree);

            var semanticModel = compilation.GetSemanticModel(tree);
            return semanticModel;
        }
    }
}
