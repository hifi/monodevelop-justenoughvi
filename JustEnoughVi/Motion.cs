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
    }
}

