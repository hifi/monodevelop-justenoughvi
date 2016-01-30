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

        public VisualMode(TextEditorData editor) : base(editor)
        {
        }

        #region implemented abstract members of ViMode

        public override void Activate()
        {
            _countString = "";
            _startLine = Editor.Caret.Line;
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
                        Editor.Caret.Line++;
                    }
                    else if (unicodeKey == 'k')
                    {
                        Editor.Caret.Line--;
                    }

                    SetSelectLines(_startLine, Editor.Caret.Line);
                }

                if (unicodeKey == 'd')
                {
                    ClipboardActions.Cut(Editor);
                    RequestedMode = Mode.Normal;
                }

                if (unicodeKey == 'y' || unicodeKey == 'Y')
                {
                    ClipboardActions.Copy(Editor);
                    RequestedMode = Mode.Normal;
                }

                if (unicodeKey == '<')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        MiscActions.RemoveIndentSelection(Editor);
                    }
                    RequestedMode = Mode.Normal;
                }

                if (unicodeKey == '>')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        MiscActions.IndentSelection(Editor);
                    }
                    RequestedMode = Mode.Normal;
                }
            }

            return false;
        }

        #endregion
    }
}

