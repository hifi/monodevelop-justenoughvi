using System;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public abstract class ViMode
    {
        protected TextEditor Editor { get; set; }
        public Mode RequestedMode { get; internal set; }

        protected ViMode(TextEditor editor)
        {
            Editor = editor;
            RequestedMode = Mode.None;
        }

        public abstract void Activate();
        public abstract void Deactivate();
        public abstract bool KeyPress (KeyDescriptor descriptor);
    }
}

