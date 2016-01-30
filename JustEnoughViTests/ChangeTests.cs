using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class ChangeTests : TextEditorTestBase
    {
        [Test]
        public void TestCase()
        {
            var editor = Create(
                @"aaaaaa
                  bbbbbb
                  ccc$ccc
                  dddddd
                  eeeeee");
            var normalMode = new NormalMode(editor);
            //data.MainSelection = data.MainSelection.WithSelectionMode (SelectionMode.Block);
            base.ProcessKeys("C", normalMode);
            Check (editor, 
                @"aaaaaa
                  bbbbbb
                  $
                  dddddd
                  eeeeee"  );
        }
    }
}

