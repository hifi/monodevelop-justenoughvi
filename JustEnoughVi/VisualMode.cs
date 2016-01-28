using System;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class VisualMode : ViMode
    {
        private string _countString;

        private int _startLineStart;
        private int _startLineEnd;

        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        public VisualMode(TextEditor editor) : base(editor)
        {
        }

        #region implemented abstract members of ViMode

        public override void Activate()
        {
            _countString = "";
            int origOffset = Editor.CaretOffset;
            Editor.CaretColumn = DocumentLocation.MinColumn;
            _startLineStart = Editor.CaretOffset;
            EditActions.MoveCaretToLineEnd(Editor);
            _startLineEnd = Editor.CaretOffset;
            Editor.CaretOffset = origOffset;
            Editor.SetSelection(_startLineStart, _startLineEnd);
        }

        public override void Deactivate()
        {
            Editor.ClearSelection();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            uint unicodeKey = descriptor.KeyChar;

            if (descriptor.ModifierKeys == 0)
            {
                // build repeat buffer
                if (unicodeKey >= '0' && unicodeKey <= '9')
                {
                    _countString += Char.ToString((char)unicodeKey);
                    return false;
                }

                if (unicodeKey == 'j' || unicodeKey == 'k')
                {
                    if (unicodeKey == 'j')
                    {
                        Editor.CaretLine++;
                    }
                    else if (unicodeKey == 'k')
                    {
                        Editor.CaretLine--;
                    }

                    int origOffset = Editor.CaretOffset;
                    Editor.CaretColumn = DocumentLocation.MinColumn;
                    int endLineStart = Editor.CaretOffset;
                    EditActions.MoveCaretToLineEnd(Editor);
                    int endLineEnd = Editor.CaretOffset;
                    Editor.CaretOffset = origOffset;

                    int selectStart = 0;
                    int selectEnd = 0;

                    if (endLineStart > _startLineEnd)
                    {
                        selectStart = _startLineStart;
                        selectEnd = endLineEnd;
                    }
                    else
                    {
                        selectStart = _startLineEnd;
                        selectEnd = endLineStart;
                    }

                    Editor.SetSelection(selectStart, selectEnd);
                }

                if (unicodeKey == 'd')
                {
                    EditActions.ClipboardCut(Editor);
                    RequestedMode = Mode.Normal;
                }

                if (unicodeKey == 'y' || unicodeKey == 'Y')
                {
                    EditActions.ClipboardCopy(Editor);
                    RequestedMode = Mode.Normal;
                }

                if (unicodeKey == '<')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        EditActions.UnIndentSelection(Editor);
                    }
                    RequestedMode = Mode.Normal;
                }

                if (unicodeKey == '>')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        EditActions.IndentSelection(Editor);
                    }
                    RequestedMode = Mode.Normal;
                }
            }

            return false;
        }

        #endregion
    }
}

