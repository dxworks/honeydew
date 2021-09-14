using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewExtractors.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpCompilationMaker : ICompilationMaker
    {
        private IEnumerable<MetadataReference> _references;

        public Compilation GetCompilation()
        {
            _references = FindTrustedReferences();

            var compilation = CSharpCompilation.Create("Compilation", references: _references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return compilation;
        }

        public IEnumerable<MetadataReference> FindTrustedReferences()
        {
            if (_references != null)
            {
                return _references;
            }

            var references = new List<MetadataReference>();

            var value = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            if (value != null)
            {
                var pathToDlls = value.Split(Path.PathSeparator);
                foreach (var reference in pathToDlls.Where(pathToDll => !string.IsNullOrEmpty(pathToDll))
                    .Select(pathToDll => MetadataReference.CreateFromFile(pathToDll)))
                {
                    references.Add(reference);
                }
            }

            _references = references;

            return references;
        }
    }
}
