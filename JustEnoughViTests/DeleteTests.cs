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
        [TestCase("a$slkdjf alsdfklasdjf", "de", " $alsdfklasdjf")]
        [TestCase("while (endOffset < searchText.Length", "5dw", ".$Length")]
        [TestCase("while (endOffset < searchText.Length", "5de", ".$Length")]
        [TestCase("this$ is a test", "dtt", "thit$est")]
        [TestCase("this$ is a test", "d2tt", "thit$")]
        [TestCase("this$ is a test", "dft", "thie$st")]
        [TestCase("aa$lkdjf alsdfklasdjf", "D", "a$")]
        [TestCase("aas$kdjf alsdfklasdjf", "d$", "aa$")]
        [TestCase("aaa$aaa\nbbbbb\nccccc\n", "dd","b$bbbb\nccccc\n")]
        [TestCase("( asl$kdjf )", "di(", "()$")]
        [TestCase("($ aslkdjf )", "di(", "()$")]
        [TestCase("{ aaa$aaaa; }", "di{", "{}$")]
        [TestCase("{ aaa$aaaa; }", "di}", "{}$")]
        [TestCase("\"as$ldkjfasf bbb\"", "di\"", "\"\"$")]
        [TestCase("\"aaaaa\" \"$bbb\"", "di\"", "\"aaaaa\" \"\"$")]
        [TestCase("\"aaaaa\"$ \"bbb\"", "di\"", "\"\"$ \"bbb\"")]
        [TestCase("\n'a'$ '\n", "di'", "\n''$ '\n")]
        [TestCase("'$a'", "di'", "''$")]
        [TestCase("'a$'", "di'","''$")] 
        [TestCase("Test(typeof(Normal$Mode()));", "di(", "Test(typeof()$);")]
        [TestCase("hello ( hello() w$hile(true) )", "di(", "hello ()$")] 
        [TestCase("<a, b$, c>\n", "di<", "<>$\n")] 
        [TestCase("\n\t<$a, b, c>\n", "di>", "\n\t<>$\n")]
        public void D_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("(int $a,\r\n int b)\r\n", "di(", "()$\r\n")]
        [TestCase("(int $a,\n int b)\n", "di)", "()$\n")]        
        [TestCase("[ aaa,\n\tbbb]$\n", "di[", "[]$\n")]
        [TestCase("[ \r\naaa,\r\n\tbbb]$\r\n", "di]", "[]$\r\n")]        
        public void should_delete_block_with_newlines(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("{\n\tint$ a;\n\tint b;\n}\n", "di{", "{\n}$\n")]
        [TestCase("{\r\n\tint$ a;\r\n\tint b;\r\n}\r\n", "di{", "{\r\n}$\r\n")]

        [TestCase("(int$ a,\n  int b\n)", "di(", "($\n)")]
        [TestCase("(int$ a,\r\n  int b\r\n)", "di(", "($\r\n)")]

        [TestCase("($\n  int b\n)", "di)", "(\n)$")] 
        [TestCase("($\r\n  int b\r\n)", "di)", "(\r\n)$")] 

        [TestCase("[\n  int b\n  int$ c]\n", "di]", "[\n]$\n")]
        [TestCase("[\r\n  int b\r\n  int$ c]\r\n", "di]", "[\r\n]$\r\n")]

        [TestCase("<aaa$\n>\n", "di>", "<$\n>\n")]
        [TestCase("<aaa$\r\n>\r\n", "di>", "<$\r\n>\r\n")]
        public void should_keep_newline(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [Test]
        public void should_keep_block_indented()
        {
            string source = @"
            {

                int$ a;
                int b;  
            }";
            string expected = @"
            {
            }$";

            Test(source, "di{", expected, typeof(NormalMode));
        }
    }
}
