using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Honeydew.Extractors.VisualBasic;

public class VisualBasicSyntacticModelCreator
{
    public SyntaxTree Create(string fileContent)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            throw new ExtractionException("Empty Content");
        }

        return VisualBasicSyntaxTree.ParseText(fileContent);
    }
}
