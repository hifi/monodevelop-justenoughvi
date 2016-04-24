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
        [TestCase("a l$ong sentence", "cw", "a $ sentence")]
        [TestCase("a l$ong sentence", "ce", "a $ sentence")]
        [TestCase("a l$ong\tsentence\n", "ce", "a $\tsentence\n")] 
        [TestCase("a l$ong long sentence", "2cw", "a $ sentence")] 
        [TestCase("a l$ong long sentence", "c2w", "a $ sentence")]
        [TestCase("a l$ong long sentence", "2ce", "a $ sentence")]
        [TestCase("a l$ong long sentence", "c2e", "a $ sentence")]
        [TestCase("al$dfklasdjf\n", "cw", "a$\n")]
        [TestCase("( abcde$fghij )", "ci(", "($)")]
        [TestCase("{ alskd$fasl }", "ci{", "{$}")]
        [TestCase("{$ alskdjfasl }", "ci{", "{$}")]
        [TestCase("($  alskdjfasl)", "ci(", "($)")]
        [TestCase("{\n\tint a$;\n\tint b;\n}", "ci{", "{\n\t$\n}")]
        [TestCase("\t(int a$,\n\t int b)\n", "ci(", "\t($)\n")]
        [TestCase("{ aksljd$f\n\t\taskldjf\n\t}", "ci{", "{$\n\t}")]
        [TestCase("\"aa$aa bbb cc\"", "ci\"", "\"$\"")]
        [TestCase("\"$aaaa \tbbb cc\"", "ci\"", "\"$\"")]
        [TestCase("\n\"aa$aaa\"", "ci\"", "\n\"$\"")]
        [TestCase("\n\"\taaa$aa\"\n", "ci\"", "\n\"$\"\n")]
        [TestCase("'a'$", "ci'", "'$'")]
        [TestCase("'a$'", "ci'", "'$'")]
        [TestCase("while ( w$hile(true) )", "ci(", "while ($)")] 
        [TestCase("while ( while($true) )", "ci(", "while ( while($) )")]
        [TestCase("aa()$\n", "ci(", "aa($)\n")]
        public void Change_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(InsertMode));
        }

        // http://vimdoc.sourceforge.net/htmldoc/motion.html#a]
        [TestCase("on($opening)", "on$")]
        [TestCase("on(closing)$", "on$")]
        [TestCase("(inside$)", "$")]
        [TestCase("nested(inside$(outer))", "nested$")]
        [TestCase("nested(inside($inner))", "nested(inside$)")]
        [TestCase("multi(\nline$\n)", "multi$")]
        public void should_change_a_block(string source, string expected)
        {
            Test(source, "ca)", expected, typeof(InsertMode));
            Test(source, "ca(", expected, typeof(InsertMode));
            Test(source, "cab", expected, typeof(InsertMode));

            source = source.Replace('(', '{').Replace(')', '}');
            expected = expected.Replace('(', '{').Replace(')', '}');

            Test(source, "ca{", expected, typeof(InsertMode));
            Test(source, "ca}", expected, typeof(InsertMode));
            Test(source, "caB", expected, typeof(InsertMode));

            source = source.Replace('{', '[').Replace('}', ']');
            expected = expected.Replace('{', '[').Replace('}', ']');

            Test(source, "ca[", expected, typeof(InsertMode));
            Test(source, "ca]", expected, typeof(InsertMode));

            source = source.Replace('[', '<').Replace(']', '>');
            expected = expected.Replace('[', '<').Replace(']', '>');

            Test(source, "ca<", expected, typeof(InsertMode));
            Test(source, "ca>", expected, typeof(InsertMode));
        }

        // http://vimdoc.sourceforge.net/htmldoc/motion.html#a'
        [TestCase("`simple$ case`", "$")]
        [TestCase("`must work`\n`on$ single line only`\n", "`must work`\n\n")]
        [TestCase("`if``on quote must search from line start`$", "`if`")]
        [TestCase("`if``$on quote must search from line start`", "`if`")]
        [TestCase("trailing white space `included$`\t   ", "trailing white space $")]
        [TestCase("...unless there's none,    `then$ leading white space is included`", "...unless there's none,$")]
        public void should_change_quoted_string(string source, string expected)
        {
            Test(source, "ca`", expected, typeof(InsertMode));

            source = source.Replace('`', '"');
            expected = expected.Replace('`', '"');

            Test(source, "ca\"", expected, typeof(InsertMode));

            source = source.Replace('"', '\'');
            expected = expected.Replace('"', '\'');

            Test(source, "ca'", expected, typeof(InsertMode));

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

