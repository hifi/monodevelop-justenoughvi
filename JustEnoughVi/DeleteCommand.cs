using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public class DeleteCommand : Command
    {
        protected Func<CommandRange> _selector;

        public DeleteCommand(TextEditorData editor, Func<TextEditorData, char, char, CommandRange> selector, char openingChar, char closingChar)
            : base(editor)
        {
            _selector = () => selector(Editor, openingChar, closingChar);
        }

        public DeleteCommand(TextEditorData editor, Func<TextEditorData, char, CommandRange> selector, char enclosingChar) : base(editor)
        {
            _selector = () => selector(Editor, enclosingChar);
        }

        protected override void Run()
        {
            var range = _selector();
            DeleteRange(range);
        }

        protected void DeleteRange(CommandRange range)
        {
            if (range.Length > 0)
            {
                Editor.SetSelection(range.Start, range.End);
                ClipboardActions.Cut(Editor);
            }
        }
    }
}
