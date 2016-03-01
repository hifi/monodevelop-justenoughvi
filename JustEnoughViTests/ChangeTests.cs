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
        [TestCase("a l$ong sentence", "ce", "a $sentence")]
        [TestCase("a l$ong\tsentence\n", "ce", "a $\tsentence\n")] 
        [TestCase("a l$ong long sentence", "2cw", "a $sentence")] 
        [TestCase("a l$ong long sentence", "c2w", "a $sentence")]
        [TestCase("a l$ong long sentence", "2ce", "a $sentence")]
        [TestCase("a l$ong long sentence", "c2e", "a $sentence")]
        [TestCase("al$dfklasdjf\n", "cw", "al$\n")]
        [TestCase("( abcde$fghij )", "ci(", "($)")]
        [TestCase("{ alskd$fasl }", "ci{", "{$}")]
        [TestCase("{$ alskdjfasl }", "ci{", "{$}")]
        [TestCase("($  alskdjfasl)", "ci(", "($)")]
        [TestCase("{\n\tint a$;\n\tint b;\n}", "ci{", "{\n\t$\n}")]
        [TestCase("\t(int a$,\n\t int b)\n", "ci(", "\t($)\n")]
        [TestCase("{ aksljd$f\n\t\taskldjf\n\t}", "ci{", "{$\n\t}")]
        [TestCase("\"aa$aa bbb cc\"", "ci\"", "\"$\"")]
        [TestCase("\"$aaaa bbb cc\"", "ci\"", "\"$\"")]
        [TestCase("'a'$", "ci'", "'$'")]
        [TestCase("'a$'", "ci'", "'$'")]
        public void Change_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(InsertMode));
        }

        [TestCase("s$at", "rc", "c$at")]
        public void R_should_replace_char(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("N$avigationTests", "Rreplacing", "Replacingn$Tests")]
        public void ReplaceModeTests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(ReplaceMode));
        }

    }
}

