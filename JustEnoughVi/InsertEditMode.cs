using System;
using Mono.TextEditor;
using MonoDevelop.SourceEditor;

namespace JustEnoughVi
{
    public class InsertEditMode : BaseEditMode
    {
        public InsertEditMode(ViEditMode vi) : base(vi)
        {
        }

        #region implemented abstract members of BaseEditMode

        public override void InternalActivate(MonoDevelop.SourceEditor.ExtensibleTextEditor editor, Mono.TextEditor.TextEditorData data)
        {
            data.Caret.Mode = CaretMode.Insert;
        }

        public override void InternalDeactivate(ExtensibleTextEditor editor, TextEditorData data)
        {
            CaretMoveActions.Left(data);
        }

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                Vi.SetMode(ViMode.Normal);
                return;
            }

            Vi.BaseKeypress(key, unicodeKey, modifier);
        }

        #endregion
    }
}

