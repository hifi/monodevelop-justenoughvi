using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class VisualMode : ViMode
    {
        private string _countString;
        private int _startLine;

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
            _startLine = Editor.CaretLine;
            SetSelectLines(_startLine, _startLine);
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

                    SetSelectLines(_startLine, Editor.CaretLine);
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

