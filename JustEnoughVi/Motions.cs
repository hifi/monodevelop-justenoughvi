using System;

using Mono.TextEditor;

namespace JustEnoughVi
{
    public abstract class Motion
    {
        //public Motion(TextEditorData editor)
        //{
        //    Editor = editor;
        //}
        //public Motion(string keybinding)
        //{
        //    KeyBinding = keybinding;
        //}
        public KeyBinding KeyBinding { get; protected set; } 
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        //public TextEditorData Editor { get; private set; }
        public abstract void Run(TextEditorData editor);
    }

    public class WholeLineMotion : Motion
    {

        public WholeLineMotion()
        {
            KeyBinding = new KeyBinding(typeof(VisualMode), "d");
        }

        public override void Run(TextEditorData editor)
        {
            var line = editor.GetLine(editor.Caret.Line);
            StartOffset = line.Offset;
            EndOffset = line.EndOffset;
        }
    }

    //TODO: ugh - maybe need to support multiple modes
    public class WholeLineNormalMotion : Motion
    {
        public WholeLineNormalMotion()
        {
            KeyBinding = new KeyBinding(typeof(NormalMode), "d");
        }

        public override void Run(TextEditorData editor)
        {
            var line = editor.GetLine(editor.Caret.Line);
            StartOffset = line.Offset;
            EndOffset = line.EndOffset;
        }
    }

    public class EndOfLineMotion : Motion
    {
        public EndOfLineMotion()
        {
            KeyBinding = new KeyBinding(typeof(NormalMode), "$");
        }

        public override void Run(TextEditorData editor)
        {
            var line = editor.GetLine(editor.Caret.Line);
            StartOffset = editor.Caret.Offset;
            EndOffset = line.EndOffset;
        }
    }

    public class WordMotion : Motion
    {
        public WordMotion()
        {
            KeyBinding = new KeyBinding(typeof(NormalMode), "w");
        }

        public override void Run(TextEditorData editor)
        {
            EndOffset = StringUtils.NextWordOffset(editor.Text, editor.Caret.Offset);
        }
    }

    public class WordBackMotion : Motion
    {
        public WordBackMotion()
        {
            KeyBinding = new KeyBinding(typeof(NormalMode), "W");
        }

        public override void Run(TextEditorData editor)
        {
            EndOffset = StartOffset;
            StartOffset = StringUtils.PreviousWordOffset(editor.Text, editor.Caret.Offset);
        }
    }
}
