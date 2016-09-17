using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public class ChangeCommand : Command
    {
        Func<CommandRange> _selector;

        public ChangeCommand(TextEditorData editor, Func<TextEditorData, char, char, CommandRange> selector, char openingChar, char closingChar) : base(editor)
        {
            _selector = () => selector(Editor, openingChar, closingChar);
        }

        public ChangeCommand(TextEditorData editor, Func<TextEditorData, char, CommandRange> selector, char enclosingChar) : base(editor)
        {
            _selector = () => selector(Editor, enclosingChar);
        }

        //public ChangeCommand(TextEditorData editor, Func<TextEditorData, CommandRange> selector) : base(editor)
        //{
        //    _selector = () => selector(Editor);
        //}

        protected override void Run()
        {
            var range = _selector();
            if (range.Length > 0)
            {
                Editor.SetSelection(range.Start, range.End);
                ClipboardActions.Cut(Editor);
            }
            if (range != CommandRange.Empty)
                RequestedMode = Mode.Insert;
        }
    }
}
