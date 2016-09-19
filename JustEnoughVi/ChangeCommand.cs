using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public class ChangeCommand : DeleteCommand
    {
        public ChangeCommand(TextEditorData editor, Func<TextEditorData, char, char, CommandRange> selector, char openingChar, char closingChar) 
            : base(editor, selector, openingChar, closingChar)
        {
        }

        public ChangeCommand(TextEditorData editor, Func<TextEditorData, char, CommandRange> selector, char enclosingChar) 
            : base(editor, selector, enclosingChar)
        {
        }

        protected override void Run()
        {
            var range = _selector();
            ChangeRange(range); 
        }

        protected void ChangeRange(CommandRange range)
        {
            DeleteRange(range);
            if (range != CommandRange.Empty)
                RequestedMode = Mode.Insert;        
        }
    }
}
