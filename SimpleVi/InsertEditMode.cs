using System;

namespace SimpleVi
{
    public class InsertEditMode : Mono.TextEditor.EditMode
    {
        ViEditMode Vi { get; set; }

        public InsertEditMode(ViEditMode vi)
        {
            Vi = vi;
        }

        #region implemented abstract members of EditMode

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

