using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class NavigationTests : TextEditorTestBase
    {
        [TestCase("aaa$", "h", "aa$a")]
        [TestCase("aaa$", "hh", "a$aa")]
        [TestCase("aaa$", "2h", "a$aa")]
        [TestCase("aaa$", "3h", "a$aa")]
        [TestCase("aa\naaa$", "3h", "aa\na$aa")]
        public void H_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("a$aa", "l", "aa$a")]
        public void L_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("aa$a\nbbbb", "j", "aaa\nbb$bb")]
        [TestCase("aa$a\nbbbb\ncccccc", "2j", "aaa\nbbbb\ncc$cccc")]
        [TestCase("aa$a\nbbbb\ncccccc", "jj", "aaa\nbbbb\ncc$cccc")]
        public void J_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("aaa\nbb$bb", "k", "aa$a\nbbbb")]
        [TestCase("aaa\nbbbb\ncc$cccc", "2k", "aa$a\nbbbb\ncccccc")]
        [TestCase("aaa\nbbbb\ncc$cccc", "kk", "aa$a\nbbbb\ncccccc")]
        public void K_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "w", "public c$lass NavigationTests : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "ww", "public class N$avigationTests : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "2w", "public class N$avigationTests : TextEditorTestBase")] //TODO
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "www", "public class NavigationTests :$ TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "wwww", "public class NavigationTests : T$extEditorTestBase")]
        [TestCase("w$hile (endOffset < searchText.Length", "4w", "while (endOffset < s$earchText.Length")]
        public void W_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "e", "public$ class NavigationTests : TextEditorTestBase")]
        [TestCase("while$ (asdf)", "e", "while ($asdf)")]
        [TestCase("($ (alsdkjf(asdf)", "e", "( ($alsdkjf(asdf)")]
        [TestCase("( $(alsdkjf(asdf)", "e", "( ($alsdkjf(asdf)")]
        [TestCase(" $  class NavigationTests : TextEditorTestBase", "ee", "   class NavigationTests$ : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "2e", "public class$ NavigationTests : TextEditorTestBase")] 
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "eeee", "public class NavigationTests :$ TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "5e", "public class NavigationTests : TextEditorTestBase$")]
        [TestCase("w$hile (endOffset < searchText.Length", "4e", "while (endOffset <$ searchText.Length")]
        public void E_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("   12345$67890", "^", "   1$234567890")]
        [TestCase("   12345$67890", "0", "$   1234567890")]
        [TestCase("abcd$efghijk", "fg", "abcdefg$hijk")]
        [TestCase("abcd$efghijk", "fz", "abcd$efghijk")]
        [TestCase("abcd$efghijk", "Fa", "a$bcdefghijk")]
        [TestCase("abcd$efghijk", "Fz", "abcd$efghijk")]
        [TestCase("123$45678", "f6", "123456$78")] //TODO: Fails because it thinks 6 is a count
        [TestCase("this$ is a test", "tt", "this is a $test")]
        [TestCase("this$ is a test", "2tt", "this is a tes$t")]
        [TestCase("this is a test$", "Tt", "this is a te$st")]
        [TestCase("this is a test again$", "2Tt", "this is a te$st again")]
        [TestCase("this is a test$", "Ft", "this is a t$est")]
        [TestCase("this is a test again$", "2Ft", "this is a t$est again")]
        public void MiscTests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("aaa$", Gdk.Key.Left, "aa$a")]
        [TestCase("a$aa", Gdk.Key.Right, "aa$a")]
        [TestCase("aa$a\nbbbb", Gdk.Key.Down, "aaa\nbb$bb")]
        [TestCase("aaa\nbb$bb", Gdk.Key.Up, "aa$a\nbbbb")]
        public void SpecialKeyTests(string source, Gdk.Key specialKey, string expected)
        {
            Test(source, specialKey, expected, typeof(NormalMode));
        }
    }
}

