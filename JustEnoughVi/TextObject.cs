using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public static class TextObject
    {
        public static CommandRange Block(TextEditorData editor, char openingChar, char closingChar)
        {
            var nestingLevel = 0;
            var start = editor.Caret.Offset;
            if (editor.Text[editor.Caret.Offset] == closingChar)
                start--;

            while (start >= 0)
            {
                var currentChar = editor.Text[start];
                if (currentChar == openingChar & nestingLevel == 0)
                    break;
                else if (currentChar == closingChar)
                    nestingLevel++;
                else if (currentChar == openingChar)
                    nestingLevel--;
                start--;
            }

            var end = start + 1;
            nestingLevel = 0;
            while (end < editor.Text.Length)
            {
                var currentChar = editor.Text[end];
                if (currentChar == closingChar & nestingLevel == 0)
                    break;
                else if (currentChar == openingChar)
                    nestingLevel++;
                else if (currentChar == closingChar)
                    nestingLevel--;
                end++;
            }

            if (start < 0 || end == editor.Text.Length || start == end)
                return CommandRange.Empty;

            return new CommandRange(start, end + 1);
        }

        public static CommandRange InnerBlock(TextEditorData editor, char openingChar, char closingChar)
        {
            var range = Block(editor, openingChar, closingChar);
            if (range.Length == 0)
                return CommandRange.Empty;
            return new CommandRange(range.Start + 1, range.End - 1);
        }

        public static CommandRange QuotedString(TextEditorData editor, char quotationChar)
        {
            var start = editor.Caret.Offset;
            var end = editor.Caret.Offset;
            var lineOffset = editor.GetLine(editor.Caret.Line).Offset;
            var lineEndOffset = editor.GetLine(editor.Caret.Line).EndOffset - 1; // Line includes \n
            if (editor.Text[start] == quotationChar)
            {
                // Check if we're on closing char
                start = lineOffset;
                var openingCandidate = -1;
                while (start < end)
                {
                    if (editor.Text[start] == quotationChar & openingCandidate != -1)
                        openingCandidate = -1;
                    else if (editor.Text[start] == quotationChar)
                        openingCandidate = start;
                    start = start + 1;
                }
                if (openingCandidate != -1)
                {
                    start = openingCandidate;
                }
                else
                {
                    // not on closing char, let's find closing one
                    start = editor.Caret.Offset;
                    end = start + 1;
                    while (end < lineEndOffset & editor.Text[end] != quotationChar)
                        end++;
                }
            }
            else 
            {
                while (start >= lineOffset & editor.Text[start] != quotationChar)
                    start--;

                while (end < lineEndOffset & editor.Text[end] != quotationChar)
                    end++;
            }

            if (start < 0 || end > lineEndOffset || start == end)
                return CommandRange.Empty;

            var endIncludingTrailingWhiteSpace = end;
            var startIncludingTrailingWhiteSpace = start;

            // expand to include all trailing white space
            while (endIncludingTrailingWhiteSpace < lineEndOffset && Char.IsWhiteSpace(editor.Text[endIncludingTrailingWhiteSpace + 1]))
                endIncludingTrailingWhiteSpace++;

            // if there's no trailing white space then include leading
            if (endIncludingTrailingWhiteSpace == end)
            {
                while (startIncludingTrailingWhiteSpace > lineOffset && Char.IsWhiteSpace(editor.Text[startIncludingTrailingWhiteSpace - 1]))
                    startIncludingTrailingWhiteSpace--;
            }

            return new CommandRange(Math.Min(start, startIncludingTrailingWhiteSpace), Math.Max(end, endIncludingTrailingWhiteSpace) + 1);
        }

        public static CommandRange InnerQuotedString(TextEditorData editor, char enclosingChar)
        {
            var range = QuotedString(editor, enclosingChar);
            if (range.Length == 0)
                return CommandRange.Empty;
            return new CommandRange(range.Start + 1, range.End - 1);
        }

        public static CommandRange CurrentWord(TextEditorData editor)
        {
            var start = editor.FindCurrentWordStart(editor.Caret.Offset);
            var end = editor.FindCurrentWordEnd(editor.Caret.Offset);
            return new CommandRange(start, end);
        }
    }
}
