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
        private List<PortableExecutableReference> _references;

        public Compilation GetCompilation()
        {
            _references ??= FindReferences();

            var compilation = CSharpCompilation.Create("Compilation");
            return compilation.AddReferences(_references);
            //
            // return _references
            //     .Aggregate(compilation, (current, reference) => current.AddReferences(reference));
        }

        public void AddReference(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    _references ??= FindReferences();

                    _references.Add(MetadataReference.CreateFromFile(path));
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        private static List<PortableExecutableReference> FindReferences()
        {
            var references = new List<PortableExecutableReference>();

            var value = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            if (value != null)
            {
                var pathToDlls = value.Split(Path.PathSeparator);
                references.AddRange(pathToDlls.Where(pathToDll => !string.IsNullOrEmpty(pathToDll))
                    .Select(pathToDll => MetadataReference.CreateFromFile(pathToDll))
                );
            }

            return references;
        }
    }
}
