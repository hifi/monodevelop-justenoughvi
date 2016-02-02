using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class DeleteTests : TextEditorTestBase
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
                  dddddd
                  eeeeee";
            Test(source, "Vjd", expected, typeof(NormalMode));
        }

        [TestCase("aa$a", "dw", "a$")]
        [TestCase("a lo$ng sentence", "D", "a l$")]
        [TestCase("aaa$", "dd", "$")]
        [TestCase("tw$o words", "dw", "tw$words")]
        [TestCase("a lon$g sentence", "d2w", "a lo$")]
        [TestCase("a lon$g sentence", "2dw", "a lo$")]
        public void D_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("aaa$", "x", "aa$")]
        [TestCase("aaa$", "xx", "a$")]
        [TestCase("aaa$", "xxx", "$")]
        [TestCase("aaa$", "xxxx", "$")]
        public void X_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}
