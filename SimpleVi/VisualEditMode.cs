using System;
using Mono.TextEditor;
using MonoDevelop.SourceEditor;

namespace SimpleVi
{
    public class VisualEditMode : BaseEditMode
    {
        private string _countString;

        public int VisualStart { get; set; }
        public int VisualEnd { get; set; }

        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        public VisualEditMode(ViEditMode vi) : base(vi)
        {
            _countString = "";
        }

        public override void InternalActivate(ExtensibleTextEditor editor, TextEditorData data)
        {
            _countString = "";
            data.Caret.Mode = CaretMode.Block;
            VisualStart = data.Caret.Line;
            VisualEnd = data.Caret.Line;
        }

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                Data.ClearSelection();
                Vi.SetMode(ViMode.Normal);
                return;
            }

            if (modifier == 0)
            {
                // build repeat buffer
                if (unicodeKey >= '0' && unicodeKey <= '9')
                {
                    _countString += Char.ToString((char)unicodeKey);
                    return;
                }

                if (unicodeKey == 'j' || unicodeKey == 'k')
                {
                    if (unicodeKey == 'j')
                    {
                        VisualEnd++;
                        Caret.Line = VisualEnd;
                    }
                    else if (unicodeKey == 'k')
                    {
                        VisualEnd--;
                        Caret.Line = VisualEnd;
                    }

                    int start = VisualStart;
                    int end = VisualEnd;

                    if (end < start)
                    {
                        end--;
                        start++;
                    }

                    Data.SetSelectLines(start, end);
                }

                if (unicodeKey == 'd')
                {
                    ClipboardActions.Cut(Data);
                    Vi.SetMode(ViMode.Normal);
                }

                if (unicodeKey == 'y' || unicodeKey == 'Y')
                {
                    ClipboardActions.Copy(Data);
                    Data.ClearSelection();
                    Vi.SetMode(ViMode.Normal);
                }

                if (unicodeKey == '<')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        RunAction(MiscActions.RemoveIndentSelection);
                    }
                    Data.ClearSelection();
                    Vi.SetMode(ViMode.Normal);
                }

                if (unicodeKey == '>')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        RunAction(MiscActions.IndentSelection);
                    }
                    Data.ClearSelection();
                    Vi.SetMode(ViMode.Normal);
                }
            }
        }

        #endregion
    }
}

