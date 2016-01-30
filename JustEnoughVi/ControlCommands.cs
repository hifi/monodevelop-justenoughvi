using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;

namespace JustEnoughVi
{
    public enum ControlCommands
    {
        Interrupt,
        PageUp,
        PageDown,
        HalfPageUp,
        HalfPageDown
    }

    public abstract class ControlCommandHandler : CommandHandler
    {
        protected JustEnoughVi Vi { get; private set; }
        protected TextEditor Editor { get;  private set; }

        protected ControlCommandHandler()
        {

        }

        protected override void Run()
        {
            Editor = IdeApp.Workbench.ActiveDocument.Editor;
            Vi = Editor.GetContent<JustEnoughVi>();
            ViRun();
        }

        protected abstract void ViRun();
    }

    public class InterruptCommand : ControlCommandHandler
    {
        protected override void ViRun()
        {
            Vi.Interrupt();
        }
    }

    public class PageUpCommand : ControlCommandHandler
    {
        protected override void ViRun()
        {
            Editor.CaretLine -= Math.Min(Editor.CaretLine - 1, 20);
            EditActions.MoveCaretToLineStart(Editor);
            Editor.CenterToCaret();
        }
    }

    public class PageDownCommand : ControlCommandHandler
    {
        protected override void ViRun()
        {
            Editor.CaretLine += Math.Min(Editor.LineCount - Editor.CaretLine, 20);
            EditActions.MoveCaretToLineStart(Editor);
            Editor.CenterToCaret();
        }
    }

    public class HalfPageUpCommand : ControlCommandHandler
    {
        protected override void ViRun()
        {
            Editor.CaretLine -= Math.Min(Editor.CaretLine - 1, 10);
            EditActions.MoveCaretToLineStart(Editor);
            Editor.CenterToCaret();
        }
    }

    public class HalfPageDownCommand : ControlCommandHandler
    {
        protected override void ViRun()
        {
            Editor.CaretLine += Math.Min(Editor.LineCount - Editor.CaretLine, 10);
            EditActions.MoveCaretToLineStart(Editor);
            Editor.CenterToCaret();
        }
    }
}

