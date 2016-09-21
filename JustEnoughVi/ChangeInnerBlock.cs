using System;
using Mono.TextEditor;
using ICSharpCode.NRefactory.Utils;
using ICSharpCode.NRefactory;

namespace JustEnoughVi
{
    public class ChangeInnerBlock : ChangeCommand
    {
        public ChangeInnerBlock(TextEditorData editor, char openingChar, char closingChar) 
            : base(editor, TextObject.InnerBlock, openingChar, closingChar)
        {
        }

        protected override void Run()
        {
            CommandRange range = _selector();
            ChangeRange(range);
            if (range != CommandRange.Empty)
            {
                // Move caret inside if it is on on opening character and block is empty
                if (range.Length == 0 && Editor.Caret.Offset < range.Start)
                    Editor.Caret.Offset++;
                else
                {
                    // if block still has two newlines inside, then drop inside block and indent
                    int del1 = NewLine.GetDelimiterLength(Editor.Text[range.Start - 1], Editor.Text[range.Start]);
                    if (del1 > 0)
                    {
                        int del2Start = range.Start - 1 + del1;
                        int del2 = NewLine.GetDelimiterLength(Editor.Text[del2Start],
                                                              Editor.Text[del2Start + 1]);
                        if (del2 > 0)
                            IndentCaretInBlock(range.Start - 2);
                    }
                }
            }
        }

        private void IndentCaretInBlock(int openingChar)
        {
            string indentation = Editor.GetLineIndent(Editor.OffsetToLineNumber(openingChar));
            if (indentation != null && indentation.Length > 0)
                Editor.Insert(Editor.Caret.Offset, indentation);
            MiscActions.InsertTab(Editor);
        }
    }
}
