using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public static class Motion
    {
        public static void LineStart(TextEditorData editor)
        {
            CaretMoveActions.LineStart(editor);

            var line = editor.GetLine(editor.Caret.Line);

            while (Char.IsWhiteSpace(editor.Text[editor.Caret.Offset]) && editor.Caret.Offset < line.EndOffset)
                editor.Caret.Offset++;
        }

        public static void LineEnd(TextEditorData editor)
        {
            CaretMoveActions.LineEnd(editor);
        }
    }
}

