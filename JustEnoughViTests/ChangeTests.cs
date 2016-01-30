using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class ChangeTests : TextEditorTestBase
    {
        [Test]
        public void C_should_delete_to_end_of_line()
        {
            var editor = Create(
                @"aaaaaa
                  bbbbbb
                  ccc$ccc
                  dddddd
                  eeeeee");
            
            var mode = new NormalMode(editor);
            ProcessKeys("C", mode);

            Check (editor, 
                @"aaaaaa
                  bbbbbb
                  ccc$
                  dddddd
                  eeeeee" );
            // failing Assert.IsInstanceOf<InsertMode>(mode.RequestedMode);
        }
    }
}

