using Mono.TextEditor;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class InsertMode : ViMode
    {
        public InsertMode(TextEditorData editor) : base(editor)
        {
        }

        #region implemented abstract members of ViMode

        protected override void Activate()
        {
        }

        protected override void Deactivate()
        {
            if (Editor.Caret.Column > MonoDevelop.Ide.Editor.DocumentLocation.MinColumn)
                CaretMoveActions.Left(Editor);
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            return true;
        }

        #endregion
    }
}

