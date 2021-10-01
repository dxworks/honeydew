using System;
using HoneydewModels;

namespace HoneydewExtractors.Core.Metrics.Extraction
{
    public abstract class LinesOfCodeCounter : ILinesOfCodeCounter
    {
        private readonly string _singleComment;
        private readonly string _multiLineCommentStart;
        private readonly string _multiLineCommentEnd;

        protected LinesOfCodeCounter(string singleComment, string multiLineCommentStart, string multiLineCommentEnd)
        {
            _singleComment = singleComment;
            _multiLineCommentStart = multiLineCommentStart;
            _multiLineCommentEnd = multiLineCommentEnd;
        }

        public LinesOfCode Count(string fileContent)
        {
            var lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var sourceLines = 0;
            var commentLines = 0;
            var emptyLines = 0;

            var isInsideMultiLineComment = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                var endMultilineCommentIndex = trimmedLine.IndexOf(_multiLineCommentEnd, StringComparison.Ordinal);
                var startMultilineCommentIndex = trimmedLine.IndexOf(_multiLineCommentStart, StringComparison.Ordinal);

                if (!isInsideMultiLineComment)
                {
                    if (IsLineEmpty(trimmedLine))
                    {
                        emptyLines++;
                        continue;
                    }
                }
                else
                {
                    if (endMultilineCommentIndex < 0)
                    {
                        commentLines++;
                        continue;
                    }
                }


                if (endMultilineCommentIndex >= 0)
                {
                    isInsideMultiLineComment = false;

                    if (endMultilineCommentIndex == trimmedLine.Length - _multiLineCommentEnd.Length)
                    {
                        commentLines++;
                        continue;
                    }

                    if (startMultilineCommentIndex > endMultilineCommentIndex)
                    {
                        trimmedLine = trimmedLine[(endMultilineCommentIndex + _multiLineCommentEnd.Length)..];
                    }
                }

                var singleLineCommentIndex = trimmedLine.IndexOf(_singleComment, StringComparison.Ordinal);

                if (singleLineCommentIndex >= 0)
                {
                    trimmedLine = trimmedLine[..singleLineCommentIndex];
                    if (IsLineEmpty(trimmedLine))
                    {
                        commentLines++;
                        continue;
                    }
                }

                startMultilineCommentIndex = trimmedLine.IndexOf(_multiLineCommentStart, StringComparison.Ordinal);
                endMultilineCommentIndex = trimmedLine.IndexOf(_multiLineCommentEnd, StringComparison.Ordinal);

                while (startMultilineCommentIndex >= 0)
                {
                    isInsideMultiLineComment = true;

                    if (endMultilineCommentIndex < 0)
                    {
                        trimmedLine = trimmedLine[..startMultilineCommentIndex];
                        break;
                    }

                    trimmedLine = trimmedLine.Remove(startMultilineCommentIndex,
                        endMultilineCommentIndex + _multiLineCommentEnd.Length - startMultilineCommentIndex);
                    isInsideMultiLineComment = false;

                    startMultilineCommentIndex = trimmedLine.IndexOf(_multiLineCommentStart, StringComparison.Ordinal);


                    endMultilineCommentIndex = trimmedLine.IndexOf(_multiLineCommentEnd, StringComparison.Ordinal);
                }

                if (IsLineEmpty(trimmedLine))
                {
                    commentLines++;
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

        private bool IsLineEmpty(string line)
        {
            return string.IsNullOrWhiteSpace(line) || line.Trim() is "{" or "}";
        }
    }
}
