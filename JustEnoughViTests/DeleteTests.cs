using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class DeleteTests : TextEditorTestBase
    {
        [Test]
        public void D_should_delete_to_end_of_line()
        {
            var editor = Create(
                @"aaaaaa
                  bb$bbbb
                  cccccc
                  dddddd
                  eeeeee");

            var mode = new NormalMode(editor);
            ProcessKeys("Vjd", mode);

            Check (editor, 
                   @"aaaaaa
                  dddddd
                  eeeeee" );
            // failing Assert.IsInstanceOf<InsertMode>(mode.RequestedMode);
        }
    }
}

