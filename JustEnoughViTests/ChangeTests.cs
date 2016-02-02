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


        [TestCase("abc$efg", "cc", "$")]
        [TestCase("abc$efg", "c$", "ab$")]
        [TestCase("a l$ong sentence", "cw", "a $sentence")]
        [TestCase("a l$ong sentence", "2cw", "a $")]
        [TestCase("a l$ong sentence", "C", "a $")]
        [TestCase("a lo$ng sentence", "c2w", "a l$")]
        //[TestCase("( abcde$fghij )", "ci(", "($)")]
        //TODO: 2cw & c2w
        public void Change_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(InsertMode));
        }

        [TestCase("s$at", "rc", "c$at")]
        public void R_should_replace_char(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}

