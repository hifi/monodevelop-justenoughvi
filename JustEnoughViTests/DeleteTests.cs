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
            //TODO - caret finishes in wrong location
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
