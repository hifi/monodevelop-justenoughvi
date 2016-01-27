using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public abstract class BaseEditMode
    {
        protected ViEditMode Vi { get; set; }
        protected TextEditor Editor { get; set; }

        protected BaseEditMode(ViEditMode vi, TextEditor editor)
        {
            Vi = vi;
            Editor = editor;
        }

        public abstract void Activate();
        public abstract void Deactivate();
        public abstract bool KeyPress (KeyDescriptor descriptor);
    }
}

