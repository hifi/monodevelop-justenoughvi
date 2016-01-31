using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class YankAndPasteTests: TextEditorTestBase
    {
        [Test]
        public void Should_delete_two_end_of_line()
        {
            //TODO - caret finishes in wrong position
            var source =
                @"aaaaaa
                  bb$bbbb
                  cccccc
                  dddddd
                  eeeeee";

            var expected =
                @"aaaaaa
                  bbbbbb
                  bbbbbb
                  cccccc
                  dddddd
                  eeeeee";
            Test(source, "yyp", expected, typeof(NormalMode));
        }

        [TestCase("a$b", "xp", "b$a")]
        public void Put_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}