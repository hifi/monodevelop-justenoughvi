using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public class ChangeCommand : DeleteCommand
    {
        public ChangeCommand(TextEditorData editor, Func<TextEditorData, char, char, CommandRange> selector, char openingChar, char closingChar) 
            : base(editor, selector, openingChar, closingChar)
        {
            _selector = () => selector(Editor, openingChar, closingChar);
        }

        public ChangeCommand(TextEditorData editor, Func<TextEditorData, char, CommandRange> selector, char enclosingChar) 
            : base(editor, selector, enclosingChar)
        {
            _selector = () => selector(Editor, enclosingChar);
        }

        protected override void Run()
        {
            var range = _selector();
            DeleteRange(range);
            if (range != CommandRange.Empty)
                RequestedMode = Mode.Insert;
        }
    }
}
