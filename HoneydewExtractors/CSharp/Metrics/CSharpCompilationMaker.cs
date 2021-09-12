using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpCompilationMaker : ICompilationMaker
    {
        private List<MetadataReference> _references;
        private readonly ILogger _logger;
        private int _trustedReferencesCount;

        public CSharpCompilationMaker(ILogger logger)
        {
            _logger = logger;
        }

        public Compilation GetCompilation()
        {
            _references ??= FindReferences();

            var compilation = CSharpCompilation.Create("Compilation", references: _references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return compilation;
        }

        public void AddReference(Compilation compilation, string referencePath)
        {
            if (File.Exists(referencePath))
            {
                try
                {
                    _references ??= FindReferences();

                    _references.RemoveRange(_trustedReferencesCount, _references.Count - _trustedReferencesCount);

                    var parent = Directory.GetParent(referencePath);

                    if (parent == null)
                    {
                        _references.Add(MetadataReference.CreateFromFile(referencePath));
                    }
                    else
                    {
                        foreach (var fileInfo in parent.GetFiles("*.dll"))
                        {
                            _references.Add(MetadataReference.CreateFromFile(fileInfo.FullName));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Log($"Could not add references from {referencePath} because {e}");
                }
            }
            else
            {
                try
                {
                    _references ??= FindReferences();

                    _references.RemoveRange(_trustedReferencesCount, _references.Count - _trustedReferencesCount);

                    foreach (var reference in compilation.References)
                    {
                        _references.Add(reference);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log($"Could not add references from {compilation.AssemblyName} because {e}");
                }
            }
        }

        private List<MetadataReference> FindReferences()
        {
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

            _trustedReferencesCount = references.Count;

            return references;
        }
    }
}
