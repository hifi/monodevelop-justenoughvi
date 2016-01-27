using System;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public abstract class BaseEditMode
    {
        protected TextEditor Editor { get; set; }
        public ViMode RequestedMode { get; internal set; }

        protected BaseEditMode(TextEditor editor)
        {
            Editor = editor;
            RequestedMode = ViMode.None;
        }

        public abstract void Activate();
        public abstract void Deactivate();
        public abstract bool KeyPress (KeyDescriptor descriptor);
    }
}

