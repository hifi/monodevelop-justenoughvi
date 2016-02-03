using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public static class Motion
    {
        public static void FirstColumn(TextEditorData editor)
        {
            editor.Caret.Column = Mono.TextEditor.DocumentLocation.MinColumn;
        }

        public static void LineStart(TextEditorData editor)
        {
            CaretMoveActions.LineStart(editor);

            var line = editor.GetLine(editor.Caret.Line);

            while (editor.Caret.Offset < line.EndOffset && Char.IsWhiteSpace(editor.Text[editor.Caret.Offset]))
                editor.Caret.Offset++;
        }

        public static void LineEnd(TextEditorData editor)
        {
            CaretMoveActions.LineEnd(editor);
        }

        public static void Up(TextEditorData editor)
        {
            CaretMoveActions.Up(editor);
        }

        public static void Down(TextEditorData editor)
        {
            CaretMoveActions.Down(editor);
        }

        public static void Left(TextEditorData editor)
        {
            if (DocumentLocation.MinColumn < editor.Caret.Column)
                CaretMoveActions.Left(editor);
        }

        public static void Right(TextEditorData editor)
        {
            if (editor.GetLine(editor.Caret.Line).EndOffset - editor.Caret.Offset - 1 > 0)
                CaretMoveActions.Right(editor);
        }

        // TODO: move this somewhere else? extend TextEditor?
        public static void SetSelectLines(TextEditorData editor, int start, int end)
        {
            start = Math.Min(start, editor.LineCount);
            end = Math.Min(end, editor.LineCount);

            var startLine = start > end ? editor.GetLine(end) : editor.GetLine(start);
            var endLine = start > end ? editor.GetLine(start) : editor.GetLine(end);

            editor.SetSelection(startLine.Offset, endLine.EndOffsetIncludingDelimiter);
        }
    }
}

