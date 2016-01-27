using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class InsertEditMode : BaseEditMode
    {
        public InsertEditMode(TextEditor editor) : base(editor)
        {
        }

        #region implemented abstract members of BaseEditMode

        public override void Activate()
        {
        }

        public override void Deactivate()
        {
            EditActions.MoveCaretLeft(Editor);
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            return true;
        }

        #endregion
    }
}

