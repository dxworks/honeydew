using Honeydew.Models;

namespace Honeydew.Extractors.VisualBasic;

public class VisualBasicLinesOfCodeCounter : ILinesOfCodeCounter
{
    private const string SingleComment = "'";

    public LinesOfCode Count(string fileContent)
    {
        var lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        var sourceLines = 0;
        var commentLines = 0;
        var emptyLines = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            var singleLineCommentIndex = trimmedLine.IndexOf(SingleComment, StringComparison.Ordinal);

            if (singleLineCommentIndex >= 0)
            {
                trimmedLine = trimmedLine[..singleLineCommentIndex];
                if (IsLineEmpty(trimmedLine))
                {
                    commentLines++;
                    continue;
                }
            }

            if (IsLineEmpty(trimmedLine))
            {
                emptyLines++;
            }
            else
            {
                sourceLines++;
            }
        }

        return new LinesOfCode
        {
            SourceLines = sourceLines,
            CommentedLines = commentLines,
            EmptyLines = emptyLines
        };
    }

    private static bool IsLineEmpty(string line)
    {
        return string.IsNullOrWhiteSpace(line);
    }
}
