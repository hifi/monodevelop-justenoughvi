using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.SourceEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleVi
{
    public enum ViMode
    {
        Normal,
        Insert,
        Visual
    }

    public class ViEditMode : Mono.TextEditor.EditMode
    {
        private Document _doc;
        private EditMode _baseMode; 
        private TextEditorData _data;
        private string _countString;
        private char? _command;
        private List<char> _commandArgs;
        private KeyCommand _keyCommands;

        public int VisualStart { get; set; }
        public int VisualEnd { get; set; }

        private ViMode Mode {get; set; }
        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        new public Document Document {
            get { return _doc; }
        }

        public EditMode BaseMode { get { return _baseMode; } }

        public ViEditMode(Document doc)
        {
            _doc = doc;
            _countString = "";
            _command = null;
            _commandArgs = new List<char>();

            var textEditor = doc.GetContent<SourceEditorView>().TextEditor;
            _baseMode = textEditor.CurrentMode;
            _data = _doc.GetContent<ITextEditorDataProvider>().GetTextEditorData();
            _keyCommands = new KeyCommand(this);

            SetMode(ViMode.Normal);
        }

        public void Action(Action<TextEditorData> a)
        {
            RunAction(a);
        }

        public void SetMode(ViMode newMode)
        {
            if (newMode == ViMode.Normal)
            {
                _data.Caret.Mode = CaretMode.Block;

                if (Mode == ViMode.Insert)
                {
                    CaretMoveActions.Left(_data);
                }
            }
            else if (newMode == ViMode.Insert)
            {
                _data.Caret.Mode = CaretMode.Insert;
            }
            else if (newMode == ViMode.Visual)
            {
                _data.Caret.Mode = CaretMode.Block;
            }

            Mode = newMode;

            // reset count string on mode switch for now
            _countString = "";
        }

        internal static bool IsEol(char c)
        {
            return (c == '\r' || c == '\n');
        }

        private bool NormalKeypress(Gdk.Key key, char unicodeKey, Gdk.ModifierType modifier)
        {
            // reset count 
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                _countString = "";
                _command = null;
                _commandArgs.Clear();
                return true;
            }

            if (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.f)
            {
                BaseKeypress(Gdk.Key.Page_Down, ' ', Gdk.ModifierType.None);
                return true;
            }

            if (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.b)
            {
                BaseKeypress(Gdk.Key.Page_Up, ' ', Gdk.ModifierType.None);
                return true;
            }

            if (modifier == 0)
            {
                if (key == Gdk.Key.Page_Down || key == Gdk.Key.Page_Up)
                {
                    return false;
                }

                // remap some function keys to Vi commands
                if (key == Gdk.Key.Home)
                    unicodeKey = '0';
                else if (key == Gdk.Key.End)
                    unicodeKey = '$';
                else if (key == Gdk.Key.Left)
                    unicodeKey = 'h';
                else if (key == Gdk.Key.Right)
                    unicodeKey = 'l';
                else if (key == Gdk.Key.Up)
                    unicodeKey = 'k';
                else if (key == Gdk.Key.Down)
                    unicodeKey = 'j';
                else if (key == Gdk.Key.Delete)
                    unicodeKey = 'x';
                else if (key == Gdk.Key.Insert)
                    unicodeKey = 'i';
                else if (key == Gdk.Key.BackSpace)
                    unicodeKey = 'h';

                if (_command == null)
                {
                    // build repeat buffer
                    if ((_countString.Length > 0 || unicodeKey > '0') && unicodeKey >= '0' && unicodeKey <= '9')
                    {
                        _countString += Char.ToString((char)unicodeKey);
                        return true;
                    }

                    _command = unicodeKey;
                }
                else
                {
                    _commandArgs.Add(unicodeKey);
                }

                // try running the key command
                if (_keyCommands.Execute(Data, Count, (char)_command, _commandArgs.ToArray()))
                {
                    // succeeded, reset everything
                    _countString = "";
                    _command = null;
                    _commandArgs.Clear();
                }
            }

            // never let the cursor sit on an EOL
            while (Mode == ViMode.Normal && IsEol(_data.Document.GetCharAt(_data.Caret.Offset)) && DocumentLocation.MinColumn < _data.Caret.Column)
                CaretMoveActions.Left(_data);

            // catch everything in normal mode
            return true;
        }

        private bool InsertKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                SetMode(ViMode.Normal);
                return true;
            }

            return false;
        }

        private bool VisualKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                Data.ClearSelection();
                SetMode(ViMode.Normal);
                return true;
            }

            if (modifier == 0)
            {
                // build repeat buffer
                if (unicodeKey >= '0' && unicodeKey <= '9')
                {
                    _countString += Char.ToString((char)unicodeKey);
                    return true;
                }

                if (unicodeKey == 'j' || unicodeKey == 'k')
                {
                    if (unicodeKey == 'j')
                    {
                        VisualEnd++;
                    }
                    else if (unicodeKey == 'k')
                    {
                        VisualEnd--;
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
                    ClipboardActions.Cut(_data);
                    SetMode(ViMode.Normal);
                }

                if (unicodeKey == 'y' || unicodeKey == 'Y')
                {
                    ClipboardActions.Copy(_data);
                    Data.ClearSelection();
                    SetMode(ViMode.Normal);
                }

                if (unicodeKey == '<')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        RunAction(MiscActions.RemoveIndentSelection);
                    }
                    Data.ClearSelection();
                    SetMode(ViMode.Normal);
                }

                if (unicodeKey == '>')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        RunAction(MiscActions.IndentSelection);
                    }
                    Data.ClearSelection();
                    SetMode(ViMode.Normal);
                }
            }

            return true;
        }

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (Data != null)
                _data = Data;

            bool handled = false;

            if (Mode == ViMode.Normal)
                handled = NormalKeypress(key, (char)unicodeKey, modifier);
            else if (Mode == ViMode.Insert)
                handled = InsertKeypress(key, unicodeKey, modifier);
            else if (Mode == ViMode.Visual)
                handled = VisualKeypress(key, unicodeKey, modifier);

            if (!handled)
                BaseKeypress(key, unicodeKey, modifier);
        }

        public void BaseKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            _baseMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
        }

        #endregion
    }
}

