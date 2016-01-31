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
            var source =
                @"aaaaaa
                  bbbbbb
                  ccc$ccc
                  dddddd
                  eeeeee";

            var expected = 
                @"aaaaaa
                  bbbbbb
                  cc$
                  dddddd
                  eeeeee";
            Test(source, "C", expected, typeof(InsertMode));
        }

        [TestCase("s$at", "rc", "c$at")]
        public void R_should_replace_char(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}

