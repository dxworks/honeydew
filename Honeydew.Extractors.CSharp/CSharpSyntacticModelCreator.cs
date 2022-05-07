using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Honeydew.Extractors.CSharp;

public class CSharpSyntacticModelCreator
{
    public SyntaxTree Create(string fileContent)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            throw new ExtractionException("Empty Content");
        }

        return CSharpSyntaxTree.ParseText(fileContent);
    }
}
