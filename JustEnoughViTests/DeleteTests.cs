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

        [TestCase("aaa$", "x", "aa$")]
        [TestCase("aaa$", "xx", "a$")]
        [TestCase("aaa$", "xxx", "$")]
        [TestCase("aaa$", "xxxx", "$")]
        public void X_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("a$ksjdf alsdfklasdjf", "dw", "a$lsdfklasdjf")]
        [TestCase("a$slkdjf alsdfklasdjf", "de", "a $alsdfklasdjf")]
        [TestCase("aa$lkdjf alsdfklasdjf", "D", "a$")]
        [TestCase("aas$kdjf alsdfklasdjf", "d$", "aa$")]
        [TestCase("aaa$aaa\nbbbbb\nccccc\n", "dd","b$bbbb\nccccc\n")]
        [TestCase("( asl$kdjf )", "di(", "()$")]
        [TestCase("($ aslkdjf )", "di(", "()$")]
        [TestCase("{ aaa$aaaa; }", "di{", "{}$")]
        [TestCase("{\n\tint$ a;\n\tint b;\n}\n", "di{", "{\n}$\n")]
        [TestCase("(int $a,\n int b)\n", "di(", "()$\n")]
        [TestCase("\"as$ldkjfasf bbb\"", "di\"", "\"\"$")]
        [TestCase("'$a'", "di'", "''$")]
        [TestCase("'a$'", "di'","''$")] 
        public void D_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}
