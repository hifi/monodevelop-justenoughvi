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
            Vi.BaseKeypress(key, unicodeKey, modifier);
        }

        #endregion
    }
}

