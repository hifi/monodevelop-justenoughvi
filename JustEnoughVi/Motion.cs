using System;
using Mono.TextEditor;
using MonoDevelop.Core;

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
            if (!Platform.IsMac)
                CaretMoveActions.Up(editor);
            else
            {
                using (var undo = editor.OpenUndoGroup())
                {
                    if (editor.Caret.Line > DocumentLocation.MinLine)
                    {
                        int visualLine = editor.LogicalToVisualLine(editor.Caret.Line);
                        int line = editor.VisualToLogicalLine(visualLine - 1);
                        editor.Caret.Line = line;
                    }
                }
            }
        }

        public static void Down(TextEditorData editor)
        {
            if (!Platform.IsMac)
                CaretMoveActions.Down(editor);
            else
            {
                using (var undo = editor.OpenUndoGroup())
                {
                    if (editor.Caret.Line < editor.Document.LineCount)
                    {
                        int nextLine = editor.LogicalToVisualLine(editor.Caret.Line);
                        int line = editor.VisualToLogicalLine(nextLine + 1);
                        editor.Caret.Line = line;
                    }
                }
            }
        }

        public static void Left(TextEditorData editor)
        {
            if (!Platform.IsMac)
            {
                if (DocumentLocation.MinColumn < editor.Caret.Column)
                    CaretMoveActions.Left(editor);
            }
            else
            {
                using (var undo = editor.OpenUndoGroup())
                {
                    if (editor.Caret.Column > DocumentLocation.MinColumn)
                        editor.Caret.Column = editor.Caret.Column - 1;
                }
            }
        }

        public static void Right(TextEditorData editor)
        {
            if (!Platform.IsMac)
            {
                if (editor.Caret.Offset < editor.GetLine(editor.Caret.Line).EndOffset)
                    CaretMoveActions.Right(editor);
            }
            else
            {
                using (var undo = editor.OpenUndoGroup())
                {
                    DocumentLine line = editor.GetLine(editor.Caret.Line);
                    if (editor.Caret.Column < line.Length)
                        editor.Caret.Column = editor.Caret.Column + 1;
                }
            }
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

