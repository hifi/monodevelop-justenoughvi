using System;
using Mono.TextEditor;
using ICSharpCode.NRefactory;

namespace JustEnoughVi
{
    public class DeleteInnerBlock : DeleteCommand
    {
        public DeleteInnerBlock(TextEditorData editor, char openingChar, char closingChar)
            : base(editor, TextObject.InnerBlock, openingChar, closingChar)
        {
        }

        protected override void Run()
        {
            var range = _selector();
            DeleteRange(range);
            if (range != CommandRange.Empty)
            {
                // Make sure there is no more than one newline left inside block
                int del1 = NewLine.GetDelimiterLength(Editor.Text[range.Start - 1], Editor.Text[range.Start]);
                if (del1 > 0)
                {
                    int del2Start = range.Start - 1 + del1;
                    int del2 = NewLine.GetDelimiterLength(Editor.Text[del2Start],
                                                          Editor.Text[del2Start+1]);
                    if (del2 > 0)
                    {
                        Editor.SetSelection(del2Start, del2Start + del2);
                        ClipboardActions.Cut(Editor);
                        // put caret on closingChar
                        int caret = Editor.Caret.Offset;
                        while (Char.IsWhiteSpace(Editor.Text[caret]))
                            caret++;
                        Editor.Caret.Offset = caret;
                    }
                }
            }
        }    
    }
}
